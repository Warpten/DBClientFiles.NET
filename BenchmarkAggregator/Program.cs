using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace BenchmarkAggregator
{
    internal class WorksheetGenerator
    {
        public ExcelWorksheet Worksheet { get; }

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
            SetHeading1(headerCell).Value = header;

            var dataCells = Worksheet.Cells[$"{columnName}3:{columnName}{valuesArray.Count + 2}"];
            SetInputCell(dataCells);

            var wrapped = values.Select(v => new ValueWrapper<T>(v));

            dataCells.LoadFromCollection(wrapped);

            ++_dataCount;
        }

        public static ExcelRange SetHeading1(ExcelRange headerCell)
        {
            headerCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerCell.Style.Font.Name = "Calibri";
            headerCell.Style.Font.Size = 13.0f;
            headerCell.Style.Font.Bold = true;
            headerCell.Style.Font.Color.SetColor(Color.FromArgb(68, 84, 106));

            return headerCell;
        }

        class ValueWrapper<T>
        {
            public T Value { get; set; }

            public ValueWrapper(T value)
            {
                Value = value;
            }
        }

        private static ExcelRange SetHeading2(ExcelRange currentCell)
        {
            currentCell.Style.Font.Color.SetColor(Color.FromArgb(68, 84, 133));
            currentCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
            currentCell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(162, 184, 225));
            return currentCell;
        }

        private static ExcelRange SetInputCell(ExcelRange currentCell)
        {
            currentCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            currentCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 205, 133));
            currentCell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            currentCell.Style.Font.Color.SetColor(Color.FromArgb(63, 63, 118));
            return currentCell;
        }

        private static ExcelRange SetCalculationCell(ExcelRange currentCell)
        {
            currentCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            currentCell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 205, 133));
            currentCell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            currentCell.Style.Font.Color.SetColor(Color.FromArgb(63, 63, 118));
            return currentCell;
        }

        public void FinalizeDocument()
        {
            var FULL_DATA_RANGE = $"$A$3:${_columns[_dataCount]}${_maxDataSize + 1}";
            var BIN_LABEL_COLUMN = 0;

            Worksheet.Cells[$"A1:{_columns[_dataCount - 1]}1"].Merge = true;
            SetHeading2(Worksheet.Cells[$"A1:{_columns[_dataCount - 1]}1"]).Value = "Benchmark results";

            Worksheet.Workbook.CalcMode = ExcelCalcMode.Automatic;

            var binColumnIndex = _dataCount + 1;

            var binFilterOffset = _dataCount + 20;
            var binStepTarget = $"{_columns[binFilterOffset + 1]}2";
            Worksheet.Cells[$"{_columns[binFilterOffset]}2"].Value = "Bin step";
            Worksheet.Cells[binStepTarget].Value = 10;

            var binheadercell = Worksheet.Cells[$"{_columns[binColumnIndex + 0]}1:{_columns[binColumnIndex + 2]}1"];
            binheadercell.Merge = true;
            binheadercell.Value = "Bin labels";
            SetHeading1(binheadercell);
            SetHeading2(Worksheet.Cells[$"{_columns[binColumnIndex]}2"]).Value = "Start";
            SetHeading2(Worksheet.Cells[$"{_columns[binColumnIndex + 1]}2"]).Value = "End";
            SetHeading2(Worksheet.Cells[$"{_columns[binColumnIndex + 2]}2"]).Value = "Label";

            var BIN_BLOCK_START_X = _dataCount + 1;
            var BIN_BLOCK_START_Y = 3;

            var currentRow = 0;
            SetCalculationCell(Worksheet.Cells[$"{_columns[binColumnIndex + 0]}{currentRow + 3}"]).Formula = CreateInitialBinFormula(binStepTarget);
            for (currentRow = 1; currentRow < _maxDataSize; ++currentRow)
            {
                var currentCell = Worksheet.Cells[$"{_columns[binColumnIndex + 0]}{currentRow + 3}"];
                var prevCellRef = $"{_columns[binColumnIndex + 0]}{currentRow + 2}";
                var binStepRef = Worksheet.Cells[binStepTarget].FullAddressAbsolute.Split('!')[1];

                var maxRange = $"MAX({FULL_DATA_RANGE})";
                var endFormula = $"IF({prevCellRef} + {binStepRef} < ROUNDUP({maxRange} / {binStepRef}, 0) * {binStepRef}, {prevCellRef} + {binStepRef}, NA())";
                    
                currentCell.Formula = endFormula;

                SetCalculationCell(currentCell);
            }

            BIN_LABEL_COLUMN = binColumnIndex + 2;

            for (currentRow = 0; currentRow < _maxDataSize; ++currentRow)
            {
                var endBoundCell = Worksheet.Cells[$"{_columns[binColumnIndex + 1]}{currentRow + 3}"];
                var currentCell = Worksheet.Cells[$"{_columns[binColumnIndex + 2]}{currentRow + 3}"];

                endBoundCell.Formula = $"{_columns[binColumnIndex + 0]}{currentRow + 3} + {binStepTarget}";
                currentCell.Formula = $@"CONCAT(""["", {_columns[binColumnIndex + 0]}{currentRow + 3}, "" - "", {_columns[binColumnIndex + 1]}{currentRow + 3} , ""["")";
                
                SetCalculationCell(currentCell);
                SetCalculationCell(endBoundCell);
            }

            var CATEGORY_COLUMN_BUCKET = binColumnIndex + 4;

            // generate data series assignments
            var dataSeriesoffset = CATEGORY_COLUMN_BUCKET;
            var dataAssignmentHeader = Worksheet.Cells[$"{_columns[dataSeriesoffset]}1:{_columns[dataSeriesoffset + 1]}1"];
            dataAssignmentHeader.Value = "Bin assignments";
            dataAssignmentHeader.Merge = true;
            SetHeading1(dataAssignmentHeader);
            var vlookuprange = $"${ _columns[BIN_BLOCK_START_X]}${BIN_BLOCK_START_Y}:${ _columns[BIN_BLOCK_START_X + _dataCount]}${BIN_BLOCK_START_Y + _maxDataSize - 1}";
            for (var i = 0; i < _dataCount; ++i)
            {
                var serieHeaderCell = Worksheet.Cells[$"{_columns[dataSeriesoffset + i]}2"];
                serieHeaderCell.Value = Worksheet.Cells[$"{_columns[i]}2"].Value;
                SetHeading2(serieHeaderCell);

                for (var j = 0; j < _maxDataSize; ++j)
                {
                    var serieValueCell = Worksheet.Cells[$"{_columns[dataSeriesoffset + i]}{j + BIN_BLOCK_START_Y}"];
                    var formula = $"VLOOKUP({_columns[i]}{j + 3}, {vlookuprange}, 3, TRUE)";
                    serieValueCell.Formula = formula;
                }
            }

            // generate actual assignment form to use as input for pivot
            var ASSIGNMENT_TABLE_START_X = dataSeriesoffset + 3;
            var ASSIGNMENT_TABLE_START_Y = 3;
            var ASSIGNMENT_TABLE_WIDTH = _dataCount + 2;
            var ASSIGNMENT_TABLE_HEIGHT = _maxDataSize;

            SetHeading1(Worksheet.Cells[$"{_columns[ASSIGNMENT_TABLE_START_X]}{ASSIGNMENT_TABLE_START_Y - 1}"]).Value = "Range";
            for (var i = 0; i < _dataCount; ++i)
                SetHeading2(Worksheet.Cells[$"{_columns[ASSIGNMENT_TABLE_START_X + i + 1]}{ASSIGNMENT_TABLE_START_Y - 1}"]).Formula = $"{_columns[i]}{ASSIGNMENT_TABLE_START_Y - 1}";
            SetHeading1(Worksheet.Cells[$"{_columns[ASSIGNMENT_TABLE_START_X + _dataCount + 1]}{ASSIGNMENT_TABLE_START_Y - 1}"]).Value = "Total frequency";

            for (var y = 0; y < ASSIGNMENT_TABLE_HEIGHT; ++y)
            {
                var cellY = $"{ASSIGNMENT_TABLE_START_Y + y}";
                var workerCell = Worksheet.Cells[$"{_columns[ASSIGNMENT_TABLE_START_X]}{cellY}"];
                SetCalculationCell(workerCell).Formula = $"{_columns[BIN_LABEL_COLUMN]}{cellY}";
                for (var x = 1; x < ASSIGNMENT_TABLE_WIDTH - 1; ++x)
                {
                    var cellX = $"{_columns[ASSIGNMENT_TABLE_START_X + x]}";
                    var cellCoord = $"{cellX}{cellY}";
                    var currentCell = Worksheet.Cells[cellCoord];

                    var bucketColumn = $"{_columns[CATEGORY_COLUMN_BUCKET + x - 1]}";
                    currentCell.Formula = $"COUNTIF(${bucketColumn}3:${bucketColumn}{_maxDataSize + 2}, \"=\" & {workerCell.Address})";
                }

                var sumCell = Worksheet.Cells[$"{_columns[ASSIGNMENT_TABLE_START_X + ASSIGNMENT_TABLE_WIDTH - 1]}{cellY}"];
                var range = $"{_columns[ASSIGNMENT_TABLE_START_X + 1]}{cellY}:{_columns[ASSIGNMENT_TABLE_START_X + _dataCount]}{cellY}";
                sumCell.Formula = "SUM(" + range + ")";
            }
            
            var PIVOT_CELL = "$AA$2";

            var PIVOT_SOURCE = $"${_columns[ASSIGNMENT_TABLE_START_X]}${ASSIGNMENT_TABLE_START_Y - 1}:${_columns[ASSIGNMENT_TABLE_START_X + ASSIGNMENT_TABLE_WIDTH - 1]}${ASSIGNMENT_TABLE_START_Y + ASSIGNMENT_TABLE_HEIGHT - 1}";

            // generate pivot
            var pivot = Worksheet.PivotTables.Add(Worksheet.Cells[PIVOT_CELL], Worksheet.Cells[PIVOT_SOURCE], Worksheet.Name + "_pivot");
            pivot.RowFields.Add(pivot.Fields["Range"]);
            for (var i = 1; i < pivot.Fields.Count; ++i)
            {
                var field = pivot.DataFields.Add(pivot.Fields[1]);
                if (i < pivot.Fields.Count - 1)
                    field.Name = Worksheet.Cells[$"{_columns[i - 1]}2"].Text;
            }

            var chart = Worksheet.Drawings.AddChart(pivot.Name + "chart", eChartType.ColumnClustered, pivot);
            chart.SetPosition(1, 0, 4, 0);
            chart.SetSize(400, 300);
            chart.Legend.Position = eLegendPosition.Bottom;

            chart.YAxis.Title.Text = "Frequency";
            chart.YAxis.Title.TextVertical = OfficeOpenXml.Drawing.eTextVerticalType.Vertical270;
            chart.YAxis.Title.Font.Size = 9;
            chart.YAxis.Title.Font.LatinFont = "Calibri (Body)";

            chart.XAxis.Title.Text = "Time (ms)";
            chart.XAxis.Title.Font.Size = 9;
            chart.XAxis.Title.Font.LatinFont = "Calibri (Body)";

            chart.Style = eChartStyle.Style1;

            chart.ShowHiddenData = false;
            chart.ShowDataLabelsOverMaximum = false;

            chart.Border.LineStyle = OfficeOpenXml.Drawing.eLineStyle.Solid;

            chart.RoundedCorners = false;

            chart.Title.Text = "Load time for " + Worksheet.Name;
            chart.Title.Font.Size = 16;
            chart.Title.Font.Bold = true;
            chart.Title.Font.LatinFont = "Calibri (Body)";

            var lastSerie = chart.Series[chart.Series.Count - 1];
            lastSerie.Fill.Transparancy = 100;
            lastSerie.Border.Width = 0;
            lastSerie.Header = "  ";
            
            pivot.MultipleFieldFilters = false;
            pivot.ShowDrill = false;
        
            Worksheet.Cells["A1:Z500"].Calculate();
        }

        private string CreateInitialBinFormula(string binStepTarget)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("=MIN(");
            for (var i = 0; i < _dataCount; ++i)
            {
                var dataSetColumn = _columns[i];
                stringBuilder.Append($"ROUNDDOWN(MIN(${dataSetColumn}$3:${dataSetColumn}${3 + _maxDataSize}) / {binStepTarget}, 0) * {binStepTarget},");
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
            using (var fs = File.OpenRead("./test.xlsx"))
            using (var excel = new ExcelPackage(fs))
            {
                Console.WriteLine(1);
            }

            File.Delete("./test.xlsx");

            var fg = new BenchmarkAggregator.XLSX.FileGenerator("./test.xlsx");
            var ws = fg.CreateWorksheet("test-worksheet");
            ws.AddSeries("test", new double[] { 1, 2, 4, 3, 7, 6 });
            ws.AddSeries("test2", new double[] { 5, 2, 1, 1 });
            ws.FinalizeWorksheet(1);
            fg.Save();
            return;

            using (var fileGenerator = new FileGenerator())
            {
                var worksheet = fileGenerator.CreateWorksheet("Test");
                worksheet.AddColumnValues("header", "A", new[] { 1, 10, 5, 7, 9, 3, 5 });
                worksheet.AddColumnValues("header2", "B", new[] { 7, 5 });
                worksheet.FinalizeDocument();
                File.Delete("./t.xlsx");
                using (var fs = File.Create("./t.xlsx"))
                    fileGenerator.Package.SaveAs(fs);
            }
        }
    }
}
