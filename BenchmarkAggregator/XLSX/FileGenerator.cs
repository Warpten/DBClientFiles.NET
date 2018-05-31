using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace BenchmarkAggregator.XLSX
{
    internal class FileGenerator
    {
        private ExcelPackage _workbook;
        private string _filename;

        public FileGenerator(string fileName)
        {
            _filename = fileName;

            _workbook = new ExcelPackage();
        }

        public Worksheet CreateWorksheet(string worksheetName)
        {
            return new Worksheet(worksheetName, _workbook);
        }

        public void Save()
        {
            using (var fs = File.OpenWrite(_filename))
                _workbook.SaveAs(fs);
        }
    }

    internal class Worksheet
    {
        private ExcelPackage _workbook;
        private ExcelWorksheet _worksheet;

        private List<SerieWrapper> _series = new List<SerieWrapper>();
        
        public Worksheet(string name, ExcelPackage workbook)
        {
            _workbook = workbook;
            _worksheet = _workbook.Workbook.Worksheets.Add(name);
        }

        public void AddSeries(string serieName, IEnumerable<double> values)
        {
            _series.Add(new SerieWrapper()
            {
                Name = serieName,
                Serie = values
            });
        }

        public void FinalizeWorksheet(int binSize = 10)
        {
            // find min and max across all series
            var minValue = 0.0;
            var maxValue = 0.0;
            foreach (var serie in _series)
            {
                foreach (var serieItem in serie.Serie)
                {
                    if (serieItem > maxValue)
                        maxValue = serieItem;

                    if (serieItem < minValue)
                        minValue = serieItem;
                }
            }

            var distance = maxValue - minValue;
            var binCount = Math.Ceiling(distance / binSize);

            var firstBin = Math.Floor(minValue / binSize) * binSize;
            // For each serie, create a new condensed bin map
            var binStorage = new Dictionary<double /* lower_bin_bound */, Dictionary<string, int> /* frequency_per_serie>*/>();

            for (var i = 0; i < binCount; ++i)
            {
                var lowerBinBound = firstBin + binSize * i;
                var higherBinBound = lowerBinBound + binSize;

                if (!binStorage.TryGetValue(lowerBinBound, out var freqStore))
                    binStorage[lowerBinBound] = freqStore = new Dictionary<string, int>();

                foreach (var serieInfo in _series)
                {
                    foreach (var serieItem in serieInfo.Serie)
                    {
                        if (serieItem >= lowerBinBound && serieItem < higherBinBound)
                        {
                            if (!freqStore.ContainsKey(serieInfo.Name))
                                freqStore[serieInfo.Name] = 0;

                            freqStore[serieInfo.Name]++;
                        }
                    }
                }
            }

            //var table = binStorage.SelectMany(binInfo =>
            //{
            //    var l = binInfo.Value.Select(s => new PivotTableEntry()
            //    {
            //        Frequency = s.Value,
            //        Name = s.Key,
            //        Range = $"[ {binInfo.Key} - {binInfo.Key + binSize} ["
            //    }).ToList();
            //    l.Add(new PivotTableEntry()
            //    {
            //        Frequency = binInfo.Value.Sum(s => s.Value),
            //        Name = "Total Frequency",
            //        Range = $"[ {binInfo.Key} - {binInfo.Key + binSize} ["
            //    });
            //    return l;
            //}).ToList();

            var table = binStorage.Select(binInfo =>
            {
                var freq = binInfo.Value.Select(v => v.Value).ToList();
                freq.Add(binInfo.Value.Sum(s => s.Value));

                var names = binInfo.Value.Select(v => v.Key).ToList();
                names.Add("Total Frequency");

                return new PivotTableEntry()
                {
                    Frequency = freq.ToArray(),
                    Name = names.ToArray(),
                    Range = $"[ {binInfo.Key} - {binInfo.Key + binSize} ["
                };
            }).ToList();

            var dataRange = _worksheet.Cells[1, 1].LoadFromCollection(table, true);
            dataRange.AutoFitColumns();

            var pivotTable = _worksheet.PivotTables.Add(_worksheet.Cells[1, _series.Count + 5], dataRange, _worksheet.Name + "_pivotTable");
            pivotTable.RowFields.Add(pivotTable.Fields["Range"]);
            pivotTable.ColumnFields.Add(pivotTable.Fields["Name"]);
            pivotTable.DataFields.Add(pivotTable.Fields["Frequency"]);
            pivotTable.TableStyle = OfficeOpenXml.Table.TableStyles.Light16;
            pivotTable.ColumnGrandTotals = false;
            pivotTable.MultipleFieldFilters = true;

            var pivotChart = _worksheet.Drawings.AddChart(_worksheet.Name + "_pivotChart", OfficeOpenXml.Drawing.Chart.eChartType.ColumnClustered, pivotTable);
            pivotChart.SetPosition(1, 0, 4, 0);
            pivotChart.SetSize(400, 200);
            pivotChart.Legend.Position = OfficeOpenXml.Drawing.Chart.eLegendPosition.Bottom;
            pivotChart.Style = OfficeOpenXml.Drawing.Chart.eChartStyle.None;
            pivotChart.RoundedCorners = false;
            pivotChart.XAxis.Title.Text = "Time (ms)";
            pivotChart.YAxis.Title.Text = "Frequency";
            pivotChart.DisplayBlanksAs = OfficeOpenXml.Drawing.Chart.eDisplayBlanksAs.Span;
            
            pivotTable.EnableDrill = false;
            pivotTable.CompactData = false;

            _worksheet.Cells[1, 1, 200, 200].Calculate();

            //var dataTable = _worksheet.Cell(1, 1).InsertTable(table, _worksheet.Name + "_table", true);
            //var pivotTable = _worksheet.PivotTables.AddNew(_worksheet.Name + "_pivotTable", dataTable.Cell(1, _series.Count + 4), dataTable.AsRange());
            //pivotTable.Theme = XLPivotTableTheme.PivotStyleLight16;

            //pivotTable.RowLabels.Add("Range");
            //pivotTable.ColumnLabels.Add("Name");
            //pivotTable.Values.Add("Frequency");
        }

        private class SerieWrapper
        {
            public string Name { get; set; }
            public IEnumerable<double> Serie { get; set; }
        }

        private class PivotTableEntry
        {
            public string Range { get; set; }
            public string[] Name { get; set; }
            public int[] Frequency { get; set; }
        }
    }
}
