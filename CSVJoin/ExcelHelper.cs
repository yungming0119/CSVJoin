using System.Collections.Generic;
using System.IO;
using System.Windows;
using OfficeOpenXml;
using OfficeOpenXml.Table;

public class ExcelHelper
{
    public void AppendDataToTable(string existingFilePath, string newFilePath, List<List<string>> newData, string tableName = "Table4")
    {
        // Set the license context
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        FileInfo existingFile = new FileInfo(existingFilePath);

        using (ExcelPackage package = new ExcelPackage(existingFile))
        {
            // Get the first worksheet in the workbook
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

            // Find the next available row
            int startRow = worksheet.Dimension.End.Row + 1;

            // Append new data to the worksheet
            for (int i = 0; i < newData.Count; i++)
            {
                for (int j = 0; j < newData[i].Count; j++)
                {
                    worksheet.Cells[startRow + i, j + 1].Value = newData[i][j];
                }
            }

            // Save the workbook as a new file
            FileInfo newFile = new FileInfo(newFilePath);
            if (newFile.Exists) { 
                newFile.Delete();
            }
            package.SaveAs(newFile);
            MessageBox.Show("Excel表格儲存至"+newFilePath);
        }
    }
}
