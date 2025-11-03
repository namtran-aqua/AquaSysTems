namespace AquaSolution.Client.Common.ExcelHelper
{
    using NPOI.HSSF.UserModel; // For .xls
    using System.Collections.Generic;
    using System;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public static class ExcelFileGenerator
    {

        //public static byte[] GenerateExcelFile(DataSet ds)
        //{
        //    var memoryStream = new MemoryStream();
        //    using (var workbook =
        //           SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
        //    {
        //        workbook.AddWorkbookPart();
        //        if (workbook.WorkbookPart != null)
        //        {
        //            workbook.WorkbookPart.Workbook = new Workbook
        //            {
        //                Sheets = new Sheets()
        //            };

        //            uint sheetId = 1;

        //            foreach (DataTable table in ds.Tables)
        //            {
        //                AddWorksheetToWorkbook(workbook.WorkbookPart, table, ref sheetId);
        //            }
        //        }
        //    }

        //    return memoryStream.ToArray();
        //}

        //private static void AddWorksheetToWorkbook(WorkbookPart workbookPart, DataTable table, ref uint sheetId)
        //{
        //    var sheetPart = workbookPart.AddNewPart<WorksheetPart>();
        //    var sheetData = new SheetData();
        //    sheetPart.Worksheet = new Worksheet(sheetData);

        //    var sheets = workbookPart.Workbook
        //        .GetFirstChild<Sheets>();
        //    string relationshipId = workbookPart.GetIdOfPart(sheetPart);

        //    if (sheets != null && sheets.Elements<Sheet>().Any())
        //    {
        //        sheetId =
        //            sheets.Elements<Sheet>().Select(s => s.SheetId!.Value)
        //                .Max() +
        //            1;
        //    }

        //    Sheet sheet = new Sheet { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
        //    if (sheets != null)
        //    {
        //        sheets.Append(sheet);
        //    }

        //    Row headerRow = new Row();

        //    Dictionary<string, uint> columnWidths = new Dictionary<string, uint>();
        //    List<String> columns = new List<string>();
        //    foreach (DataColumn column in table.Columns)
        //    {
        //        columns.Add(column.ColumnName);

        //        Cell cell = new Cell
        //        {
        //            DataType = CellValues.String,
        //            CellValue = new CellValue(column.ColumnName)
        //        };
        //        headerRow.AppendChild(cell);
        //        columnWidths[column.ColumnName] = (uint)column.ColumnName.Length;
        //    }

        //    sheetData.AppendChild(headerRow);

        //    foreach (DataRow dsRow in table.Rows)
        //    {
        //        Row newRow = new Row();
        //        foreach (String col in columns)
        //        {
        //            Cell cell = new Cell
        //            {
        //                DataType = CellValues.String,
        //                CellValue = new CellValue(dsRow[col].ToString() ?? string.Empty),
        //                StyleIndex = 1
        //            };
        //            newRow.AppendChild(cell);
        //            uint currentLength = (uint)(dsRow[col].ToString()?.Length ?? 0);
        //            if (columnWidths.ContainsKey(col))
        //            {
        //                if (currentLength > columnWidths[col])
        //                {
        //                    columnWidths[col] = currentLength;
        //                }
        //            }
        //            else
        //            {
        //                columnWidths[col] = currentLength;
        //            }
        //        }

        //        sheetData.AppendChild(newRow);
        //        Columns columnsElement = new Columns();
        //        foreach (var colWidth in columnWidths)
        //        {
        //            var maxColumnApply = colWidth.Value + 2;
        //            if (60 < maxColumnApply)
        //            {
        //                maxColumnApply = 60;
        //            }
        //            columnsElement.Append(new Column
        //            {
        //                Min = (uint)columns.IndexOf(colWidth.Key) + 1,
        //                Max = (uint)columns.IndexOf(colWidth.Key) + 1,
        //                Width = maxColumnApply,
        //                CustomWidth = true,
        //                BestFit = true,
        //            });
        //        }
        //        sheetPart.Worksheet.InsertBefore(columnsElement, sheetPart.Worksheet.GetFirstChild<SheetData>());
        //    }
        //}


        //----------------------.xls-----------------------
        public static byte[] GenerateExcelFile<T>(List<T> items, string? dateTimeFormat = null, params string[] excludeProperties)
        {
            var dataSet = ToDataSet(items, dateTimeFormat, excludeProperties);
            return GenerateXlsFile(dataSet);
        }

        private static byte[] GenerateXlsFile(DataSet ds)
        {
            var workbook = new HSSFWorkbook(); // .xls

            foreach (DataTable table in ds.Tables)
            {
                var sheet = workbook.CreateSheet(table.TableName);

                //----------------------------Headers----------------------------
                var headerStyle = workbook.CreateCellStyle();
                headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.BlueGrey.Index;
                headerStyle.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;

                var font = workbook.CreateFont();
                font.IsBold = true;
                font.Color = NPOI.HSSF.Util.HSSFColor.White.Index;
                headerStyle.SetFont(font);

                var headerRow = sheet.CreateRow(0);
                for (int col = 0; col < table.Columns.Count; col++)
                {
                    var cell = headerRow.CreateCell(col);
                    cell.SetCellValue(table.Columns[col].ColumnName);
                    cell.CellStyle = headerStyle; // áp dụng style
                }

                //--------------------------------Data Rows----------------------
                for (int row = 0; row < table.Rows.Count; row++)
                {
                    var dataRow = sheet.CreateRow(row + 1);
                    for (int col = 0; col < table.Columns.Count; col++)
                    {
                        var cellValue = table.Rows[row][col]?.ToString() ?? "";
                        dataRow.CreateCell(col).SetCellValue(cellValue);
                    }
                }

                //--------------------------------Set Column Width----------------
                for (int col = 0; col < table.Columns.Count; col++)
                {
                    //sheet.SetColumnWidth(col, 20 * 256); 
                }
            }

            using var ms = new MemoryStream();
            workbook.Write(ms);
            return ms.ToArray();
        }


        private static DataSet ToDataSet<T>(List<T> items, string? dateTimeFormat = null, params string[] excludeProperties)
        {
            DataSet ds = new DataSet { Locale = CultureInfo.InvariantCulture };
            DataTable dt = new DataTable(typeof(T).Name);
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                              .Where(p => excludeProperties == null || !excludeProperties.Contains(p.Name))
                                              .ToArray();
            foreach (var prop in props)
            {
                dt.Columns.Add(prop.Name);
            }

            foreach (var item in items)
            {
                var values = new object?[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    if (!string.IsNullOrEmpty(dateTimeFormat) && props[i].GetValue(item) is DateTime dtVal)
                    {
                        values[i] = dtVal.ToString(dateTimeFormat);
                    }
                    else
                    {
                        values[i] = props[i].GetValue(item);
                    }
                }
                dt.Rows.Add(values);
            }

            ds.Tables.Add(dt);
            return ds;
        }
        //-----------------------------------------------------------------
    }
}
