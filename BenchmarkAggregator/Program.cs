using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BenchmarkAggregator
{
    internal class WorksheetGenerator
    {
        public ExcelWorksheet Worksheet { get; private set; }

        private static string _columns = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private int _dataCount;
        private int _maxDataSize;
        
        public WorksheetGenerator(string name, FileGenerator parent)
        {
            Worksheet = parent.Package.Workbook.Worksheets.Add(name);
        }

        public void AddColumnValues<T>(string header, string columnName, IEnumerable<T> values)
        {
            var valuesArray = values.ToList();
            _maxDataSize = Math.Max(valuesArray.Count, _maxDataSize);

            var headerCell = Worksheet.Cells[$"{columnName}2"];
            headerCell.Value = header;
            headerCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerCell.Style.Font.Name = "Calibri";
            headerCell.Style.Font.Size = 13.0f;
            headerCell.Style.Font.Bold = true;
            headerCell.Style.Font.Color.SetColor(Color.FromArgb(68, 84, 106));

            var dataCells = Worksheet.Cells[$"{columnName}3:{columnName}{valuesArray.Count + 3}"];
            dataCells.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            dataCells.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 204, 153));
            dataCells.Style.Font.Color.SetColor(Color.FromArgb(63, 63, 118));
            dataCells.LoadFromCollection(valuesArray);

            ++_dataCount;
        }

        public void FinalizeDocument()
        {
            Worksheet.Cells[$"A1:{_columns[_dataCount]}1"].Merge = true;
            Worksheet.Cells["A1"].Value = "Benchmark results";

            var binColumnIndex = _dataCount + 2;
            var binStartColumn = _columns[binColumnIndex];

            var binFilterOffset = _dataCount + 25;
            var binStepTarget = $"{_columns[binFilterOffset + 1]}2";
            Worksheet.Cells[$"{_columns[binFilterOffset]}2"].Value = "Bin step";
            Worksheet.Cells[binStepTarget].Value = 10;

            Worksheet.Cells[$"{_columns[binStartColumn + 0]}2:{_columns[binStartColumn + 3]}"].Value = "Bin labels";
            Worksheet.Cells[$"{_columns[binStartColumn + 1]}2"].Value = "Start";
            Worksheet.Cells[$"{_columns[binStartColumn + 2]}2"].Value = "End";
            Worksheet.Cells[$"{_columns[binStartColumn + 3]}2"].Value = "Label";

            var currentRow = 0;
            Worksheet.Cells[$"{_columns[binStartColumn + 0]}{currentRow + 3}"].Formula = CreateInitialBinFormula(binStepTarget);
            for (currentRow = 1; currentRow < _maxDataSize; ++currentRow)
                Worksheet.Cells[$"{_columns[binStartColumn + 0]}{currentRow + 3}"].Formula = $"={_columns[binStartColumn + 0]}{currentRow + 2} + {binStepTarget}";

            for (currentRow = 0; currentRow < _maxDataSize; ++currentRow)
            {
                Worksheet.Cells[$"{_columns[binStartColumn + 1]}{currentRow + 3}"].Formula = $"={_columns[binStartColumn + 0]}{currentRow + 3} + {binStepTarget}";

                Worksheet.Cells[$"{_columns[binStartColumn + 2]}{currentRow + 3}"].Formula =
                    $@"=CONCAT(""["", {_columns[binStartColumn + 0]}{currentRow + 3}, "" - "", {_columns[binStartColumn + 1]}{currentRow + 3} , ""[""";
            }
        }

        private string CreateInitialBinFormula(string binStepTarget)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("=MIN(");
            for (var i = 0; i < _dataCount; ++i)
            {
                var dataSetColumn = _columns[i];
                stringBuilder.Append($"ROUNDDOWN(MIN(${dataSetColumn}$3:${dataSetColumn}{3 + _maxDataSize})/{binStepTarget},0)*{binStepTarget},");
            }

            var str = stringBuilder.ToString();
            return str.Substring(0, str.Length - 1) + ")";
        }

    }

    internal class FileGenerator : IDisposable
    {
        public ExcelPackage Package { get; private set; }

        public FileGenerator()
        {
            Package = new ExcelPackage();
        }

        public WorksheetGenerator CreateWorksheet(string worksheetName)
        {
            return new WorksheetGenerator(worksheetName, this);
        }

        public void Dispose()
        {
            Package.Dispose();
        }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            using (var excelPackage = new ExcelPackage())
            {
            }

            using (var document =
                SpreadsheetDocument.Open(@"C:\Users\Vincent Piquet\Desktop\DBFC.NET Performance.xlsx", true))
            {
                foreach (var worksheets in document.WorkbookPart.WorksheetParts)
                    InspectWorksheet(worksheets);
            }
        }
        
        private static void InspectWorksheet(WorksheetPart worksheet)
        {
            foreach (var sheet in worksheet)
            {

            }
        }
    }
}
