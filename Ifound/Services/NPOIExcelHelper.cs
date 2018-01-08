using System;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Formula.Eval;
using System.Data;
using System.IO;
using NPOI.HSSF.UserModel;

namespace Ifound.Services
{
    class NPOIExcelHelper
    {
        public static DataTable ReadExcelToDataTable(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                var workbook = default(IWorkbook);
                if (filename.IndexOf(".xlsx", StringComparison.OrdinalIgnoreCase) > 0)
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(workbook.ActiveSheetIndex); //0??指定Sheet
                if (Equals(sheet, null))
                    return null;

                var dt = new DataTable();
                CreateDataColumn(sheet, dt);
                CreateDataTable(sheet, dt);
                return dt;
            }
        }

        private static void CreateDataTable(ISheet sheet, DataTable dt)
        {
            for (var i = 1; i < sheet.LastRowNum; i++)
            {
                var sheetRow = sheet.GetRow(i);
                var dataRow = dt.NewRow();
                for (var j = 0; j < sheetRow.LastCellNum; j++)
                {
                    dataRow[j] = sheetRow.GetCell(j).ToString();
                }
                dt.Rows.Add(dataRow);
            }
                throw new NotImplementedException();
        }

        private static void CreateDataColumn(ISheet sheet, DataTable dt)
        {
            var firstRow = sheet.GetRow(0);
            for (var i = 0; i < firstRow.LastCellNum; i++)
            {
                var column = new DataColumn(firstRow.GetCell(i).StringCellValue);
                dt.Columns.Add(column);
            }
                throw new NotImplementedException();
        }
    }
}
