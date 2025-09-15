using Microsoft.AspNetCore.Components.Forms;
using OfficeOpenXml;
public static class ExcelImportHelper
{
    // Đọc Excel từ Stream thay vì IFormFile
    public static async Task<List<T>> ReadFromExcelAsync<T>(Stream excelStream, Func<ExcelWorksheet, int, T> mapFunc)
    {
        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(excelStream);
            var sheet = package.Workbook.Worksheets.FirstOrDefault();

            if (sheet == null)
                throw new Exception("No worksheet found.");

            var rowCount = sheet.Dimension.Rows;
            var result = new List<T>();

            for (int row = 2; row <= rowCount; row++)
            {
                var item = mapFunc(sheet, row);
                result.Add(item);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    // Chuyển IBrowserFile thành MemoryStream (giữ stream sống)
    public static async Task<MemoryStream> ToMemoryStreamAsync(this IBrowserFile browserFile)
    {
        try
        {
            var ms = new MemoryStream();
            using var stream = browserFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            await stream.CopyToAsync(ms);
            ms.Position = 0;
            return ms;
        }
        catch(Exception ex)
        {
            throw ex;
        }

    }
}
