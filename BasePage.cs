using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using InformSoyuzGKH.DBModel.Repository;

/// <summary>
/// Summary description for BasePage
/// </summary>
public class BasePage : System.Web.UI.Page
{
    #region Constants
    protected const string  sCssFieldHeader = "fieldHeader",
                            sCssFieldContent = "fieldContent",
                            sCssFieldContainer = "fieldContainerHeader",
                            
                            sCssFieldTable = "company_table",
                            sCssVerticalTable = "vertical_table",
                            sCssFieldTableRowHeader = "fieldTableRowHeader",
                            sCssFieldTableColHeader = "fieldTableColHeader";

    #endregion

    public BasePage()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    protected void ShowFieldTable(IList<FieldData> items, HtmlTable table, IBindingList dataRows, Control parentControl = null)
    {
        //if data is present
        if (dataRows != null && dataRows.Count > 0)
        {
            var horizontalItems = items.Where(i => i.Orientation == FieldOrientations.Horizontal).ToList();
            var verticalItems = items.Where(i => i.Orientation == FieldOrientations.Vertical).ToList();

            //get max header row count:
            Func<IList<FieldData>, int> getMaxDeepCount = null;
            getMaxDeepCount = (list) =>
            {
                int result = 0;
                foreach (var item in list)
                    if (item.Items != null)
                        result = Math.Max(result, getMaxDeepCount(item.Items));
                return result + 1;
            };

            //get count of all nested items:
            Func<IList<FieldData>, int> getAllItemsCount = null;
            getAllItemsCount = (list) =>
            {
                int result = 0;

                foreach (var item in list)
                {
                    if (item.Items != null && item.Items.Count > 0)
                    {
                        result += getAllItemsCount(item.Items);
                    }
                    else
                        result += 1;
                }

                return result;
            };

            //show table headers and get full column list:
            var columnList = ShowHorizontalTableHeader(horizontalItems, table, new HtmlTableRow(), getMaxDeepCount(horizontalItems));

            var maxVertItemsCount = 0;
            if (verticalItems.Count > 0)
                maxVertItemsCount = getAllItemsCount(verticalItems);

            //add data rows to the table:
            foreach (var dataRow in dataRows)
            {
                HtmlTableRow row = new HtmlTableRow();

                foreach (var field in columnList)
                {
                    row.Cells.Add(new HtmlTableCell() { InnerText = GetRecordText(dataRow, field.Name, field.StringFormat) });
                }

                table.Rows.Add(row);

            //if vertical items exist:
                if (verticalItems.Count > 0)
                {
                    var parentRow = new HtmlTableRow();
                    var cell = new HtmlTableCell();
                    var vertTable = new HtmlTable();
                    vertTable.Attributes.Add("Class", sCssVerticalTable);

                    var vRows = new List<HtmlTableRow>();

                    for (int i = 0; i < maxVertItemsCount; i++)
                    {
                        var vRow = new HtmlTableRow();
                        vRow.Attributes.Add("Class", sCssFieldTableColHeader); //add the class name for css customize

                        vRows.Add(vRow);
                    }

                    var rowList = ShowVerticalTableHeader(verticalItems, vertTable, vRows, 0, getMaxDeepCount(verticalItems));

                    for (int i = 0; i < rowList.Count; i++)
                    {
                        vRows[i].Cells.Add(new HtmlTableCell() { InnerText = GetRecordText(dataRow, rowList[i].Name, rowList[i].StringFormat) });
                        vertTable.Rows.Add(vRows[i]);
                    }

                    cell.Controls.Add(vertTable);
                    cell.ColSpan = columnList.Count;
                    parentRow.Cells.Add(cell);
                    table.Rows.Add(parentRow);
                }
            }

            
        }
        else if (parentControl != null)
        {
            table.Visible = false;
            parentControl.Controls.Add(new Label() { Text = ClassDB.sEmptyData }); //TODO: add class for css-handling
        }
    }
    protected List<FieldData> ShowVerticalTableHeader(IList<FieldData> items, HtmlTable table, List<HtmlTableRow> rows, int rowId, int deepMaxCount)
    {
        List<FieldData> result = new List<FieldData>();
        List<int> colSpans = Enumerable.Repeat(0, items.Count).ToList();
        List<HtmlTableCell> colCells = new List<HtmlTableCell>();

        

        var id = rowId;

        HtmlTableRow innerRow = new HtmlTableRow();

        for (int i = 0; i < items.Count; i++)
        {
            var cell = new HtmlTableCell();
            cell.Attributes.Add("Class", sCssFieldTableColHeader); //add the class name for css customize
            cell.InnerText = items[i].Header;
            rows[id].Cells.Add(cell);

            var deepList = new List<FieldData>();

            if (items[i].Items != null && items[i].Items.Count > 0)
            { //has childs
                deepList = ShowVerticalTableHeader(items[i].Items, table, rows, id, deepMaxCount - 1);
                //colSpans[i] = 1; //any constant <> 0
                result.AddRange(deepList);

                cell.ColSpan = 1; //always
                cell.RowSpan = deepList.Count;
            }
            else //alone
            {
                cell.ColSpan = deepMaxCount;
                result.Add(items[i]);
            }

            id += deepList.Count > 0 ? deepList.Count : 1;
        }

        //for (int i = 0; i < colCells.Count; i++)
        //{
        //    if (colSpans[i] == 0)
        //    {
        //        colCells[i].RowSpan = deepMaxCount;
        //        colCells[i].ColSpan = 1; //always for childless
        //    }
        //    row.Cells.Add(colCells[i]);
        //}

        return result;
    }
    protected List<FieldData> ShowHorizontalTableHeader(IList<FieldData> items, HtmlTable table, HtmlTableRow row, int deepMaxCount)
    {
        List<FieldData> result = new List<FieldData>();
        List<int> rowSpans = Enumerable.Repeat(0, items.Count).ToList();
        List<HtmlTableCell> rowCells = new List<HtmlTableCell>();

        row.Attributes.Add("Class", sCssFieldTableRowHeader); //add the class name for css customize
        table.Rows.Add(row); //adding at first, else header rows will be inverse
        HtmlTableRow innerRow = new HtmlTableRow();

        for (int i = 0; i < items.Count; i++)
        {
            rowCells.Add(new HtmlTableCell());

            if (items[i].Items != null && items[i].Items.Count > 0)
            { //has childs
                var deepList = ShowHorizontalTableHeader(items[i].Items, table, innerRow, deepMaxCount - 1);
                rowSpans[i] = 1; //any constant <> 0
                result.AddRange(deepList);

                rowCells[i].ColSpan = deepList.Count;
                rowCells[i].RowSpan = 1; //always
            }
            else //alone
            {
                result.Add(items[i]);
            }

            rowCells[i].InnerText = items[i].Header;
        }

        for (int i = 0; i < rowCells.Count; i++)
        {
            if (rowSpans[i] == 0)
            {
                rowCells[i].RowSpan = deepMaxCount;
                rowCells[i].ColSpan = 1; //always for childless
            }
            row.Cells.Add(rowCells[i]);
        }

        return result;
    }
    /*
    protected void ShowFieldTable(IList<FieldData> items, HtmlTable table, IBindingList dataRows, Control parentControl = null)
    {
        //if data is present
        if (dataRows != null && dataRows.Count > 0)
        {
            //get max header row count:
            Func<IList<FieldData>, int> getMaxDeepCount = null;
            getMaxDeepCount = (list) =>
            {
                int result = 0;
                foreach (var item in list)
                    if (item.Items != null)
                        result = Math.Max(result, getMaxDeepCount(item.Items));
                return result + 1;
            };

            //show table headers and get full column list:
            var columnList = ShowTableHeader(items, table, new HtmlTableRow(), getMaxDeepCount(items));

            //add data rows to the table:
            foreach (var dataRow in dataRows)
            {
                HtmlTableRow row = new HtmlTableRow();

                foreach (var field in columnList)
                {
                    row.Cells.Add(new HtmlTableCell() { InnerText = GetRecordText(dataRow, field.Name, field.StringFormat) });
                }

                table.Rows.Add(row);
            }
        }
        else if (parentControl != null)
        {
            table.Visible = false;
            parentControl.Controls.Add(new Label() { Text = ClassDB.sEmptyData }); //TODO: add class for css-handling
        }
    }
    protected List<FieldData> ShowTableHeader(IList<FieldData> items, HtmlTable table, HtmlTableRow row, int deepMaxCount)
    {
        List<FieldData> result = new List<FieldData>();
        List<int> rowSpans = Enumerable.Repeat(0, items.Count).ToList();
        List<HtmlTableCell> rowCells = new List<HtmlTableCell>();

        row.Attributes.Add("Class", sCssFieldTableRowHeader); //add the class name for css customize
        table.Rows.Add(row); //adding at first, else header rows will be inverse
        HtmlTableRow innerRow = new HtmlTableRow();

        for (int i = 0; i < items.Count; i++)
        {
            rowCells.Add(new HtmlTableCell());

            if (items[i].Items != null && items[i].Items.Count > 0)
            { //has childs
                var deepList = ShowTableHeader(items[i].Items, table, innerRow, deepMaxCount - 1);
                rowSpans[i] = 1; //any constant <> 0
                result.AddRange(deepList);

                rowCells[i].ColSpan = deepList.Count;
                rowCells[i].RowSpan = 1; //always
            }
            else //alone
            {
                result.Add(items[i]);
            }

            rowCells[i].InnerText = items[i].Header;
        }

        for (int i = 0; i < rowCells.Count; i++)
        {
            if (rowSpans[i] == 0)
            {
                rowCells[i].RowSpan = deepMaxCount;
                rowCells[i].ColSpan = 1; //always for childless
            }
            row.Cells.Add(rowCells[i]);
        }

        return result;
    }
    */
    protected void ShowFieldItems(FieldData item, Control div, object row, int i = 0)
    {
        if (item == null)
            return;

        bool hasItems = item.Items != null && item.Items.Count > 0;

        div.Controls.Add(new Label() { Text = item.Header, CssClass = hasItems ? sCssFieldContainer + i.ToString() : sCssFieldHeader + i.ToString() });

        if (!String.IsNullOrEmpty(item.Name))
            div.Controls.Add(new Label() { Text = GetRecordText(row, item.Name, item.StringFormat), CssClass = sCssFieldContent + i.ToString() });

        div.Controls.Add(new LiteralControl("<br />"));

        if (hasItems)
        {
            foreach (var subItem in item.Items)
                ShowFieldItems(subItem, div, row, i + 1);
        }
    }
    protected string GetRecordText(object row, string column, string format = "", bool withoutExceptions = true) //TODO: change to withoutExceptions = false
    {
        string result = null;

        try
        {
            result = DBRepository.GetStringValue(row, column, format);
        }
        catch
        {
            if (!withoutExceptions) throw;
        }

        return !String.IsNullOrEmpty(result) ? result : ClassDB.sEmptyData;
    }
}