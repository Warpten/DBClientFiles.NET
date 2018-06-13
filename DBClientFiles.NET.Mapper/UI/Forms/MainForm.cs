using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DBClientFiles.NET.Attributes;
using DBClientFiles.NET.Definitions;
using DBClientFiles.NET.Definitions.Parsers;
using DBClientFiles.NET.Mapper.Definitions;
using DBClientFiles.NET.Mapper.Mapping;

namespace DBClientFiles.NET.Mapper.UI.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private readonly Dictionary<string, DBD> _definitionContainer = new Dictionary<string, DBD>();

        private FileAnalyzer _sourceFileAnalyzer;
        private FileAnalyzer _targetFileAnalyzer;
        private string _sourceFile => textBox1.Text;
        private string _targetFile => textBox2.Text;
        private Stream _sourceStream;
        private Stream _targetStream;

        private void SelectSourceFile(object sender, EventArgs e)
        {
            _sourceStream?.Dispose();
            sourceGridView.Rows.Clear();

            var fileName = string.Empty;
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select the file to use as reference.";
                dialog.Filter = "DBC files (*.dbc, *.db2)|*.dbc;*.db2";
                if (dialog.ShowDialog() == DialogResult.OK)
                    fileName = dialog.FileName;
            }

            if (string.IsNullOrEmpty(fileName))
                return;

            textBox1.Text = fileName;

            var definitionName = Path.GetFileNameWithoutExtension(fileName);
            if (!_definitionContainer.TryGetValue(definitionName, out var definitionStore) || definitionStore == null)
                definitionStore = _definitionContainer[definitionName] = DefinitionFactory.Open(definitionName);

            _sourceStream = File.OpenRead(fileName);
            var fileAnalyzer = AnalyzerFactory.Create(_sourceStream);

            var fileType = definitionStore[fileAnalyzer.LayoutHash];
            _sourceFileAnalyzer = AnalyzerFactory.Create(fileType, _sourceStream);

            // Add to GUI
            ShowTypeToGUI(_sourceFileAnalyzer.RecordType, sourceGridView);
        }

        private void MapFiles(object sender, EventArgs e)
        {
            if (_sourceFileAnalyzer == null || _targetFileAnalyzer == null)
                return;

            var sourceFile = Path.GetFileNameWithoutExtension(_sourceFile);
            var targetFile = Path.GetFileNameWithoutExtension(_targetFile);
            if (sourceFile != targetFile)
                return;

            targetGridView.Rows.Clear();

            var mappingResolver = new MappingResolver(sourceFile, _sourceFileAnalyzer, _targetFileAnalyzer);
            foreach (var mapping in mappingResolver)
            {
                var sourceName = mapping.Value.From.Name;
                var dataRow = new DataGridViewRow();

                var dataType = (mapping.Key as PropertyInfo)?.PropertyType;
                if (dataType == null)
                    throw new InvalidOperationException("unreachable");

                var arraySize = 0;
                if (dataType.IsArray)
                {
                    dataType = dataType.GetElementType();
                    arraySize = mapping.Key.GetCustomAttribute<CardinalityAttribute>().SizeConst;
                }

                var isIndex = mapping.Key.IsDefined(typeof(IndexAttribute), false);

                dataRow.Cells.Add(new DataGridViewTextBoxCell { Value = sourceName });
                dataRow.Cells.Add(new DataGridViewTextBoxCell { Value = dataType.ToString() });
                dataRow.Cells.Add(new DataGridViewTextBoxCell { Value = arraySize == 0 ? "" : arraySize.ToString() });
                dataRow.Cells.Add(new DataGridViewCheckBoxCell { Value = isIndex });
                targetGridView.Rows.Add(dataRow);
            }
            Console.WriteLine("Mapped");
        }

        private void SelectTargetFile(object sender, EventArgs e)
        {
            _targetStream?.Dispose();
            targetGridView.Rows.Clear();

            var fileName = string.Empty;
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select the file to use as reference.";
                dialog.Filter = "DBC files (*.dbc, *.db2)|*.dbc;*.db2";
                if (dialog.ShowDialog() == DialogResult.OK)
                    fileName = dialog.FileName;
            }

            if (string.IsNullOrEmpty(fileName))
                return;

            textBox2.Text = fileName;

            _targetStream = File.OpenRead(fileName);
            _targetFileAnalyzer = AnalyzerFactory.Create(_targetStream);

            if (Properties.Settings.Default.LoadTargetDefinition)
            {
                var definitionName = Path.GetFileNameWithoutExtension(fileName);
                if (!_definitionContainer.TryGetValue(definitionName, out var definitionStore) || definitionStore == null)
                    definitionStore = _definitionContainer[definitionName] = DefinitionFactory.Open(definitionName);

                var fileType = definitionStore[_targetFileAnalyzer.LayoutHash];
                _targetFileAnalyzer = AnalyzerFactory.Create(fileType, _targetStream);

                // Add to GUI
                ShowTypeToGUI(_targetFileAnalyzer.RecordType, targetGridView);
            }
            else
            {
                var definitionName = Path.GetFileNameWithoutExtension(fileName);
                if (_definitionContainer.TryGetValue(definitionName, out var definitionStore))
                    if (definitionStore.ContainsKey(_targetFileAnalyzer.LayoutHash))
                        throw new InvalidOperationException("Structure is already known");
            }
        }

        private void OpenSettingsForm(object sender, EventArgs e)
        {
            new SettingsForm().ShowDialog();
        }

        private void OnFirstShow(object sender, EventArgs e)
        {
            sourceGridView.Columns.Add("name", "Name");
            sourceGridView.Columns.Add("type", "Type");
            sourceGridView.Columns.Add("arraySize", "Cardinality");
            sourceGridView.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "isIndex",
                HeaderText = "IsIndex",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
            });

            targetGridView.Columns.Add("name", "Name");
            targetGridView.Columns.Add("type", "Type");
            targetGridView.Columns.Add("arraySize", "Cardinality");
            targetGridView.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "isIndex",
                HeaderText = "IsIndex",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader
            });
        }

        private void ShowTypeToGUI(Type type, DataGridView gridView)
        {
            gridView.Rows.Clear();

            foreach (var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var isIndex = propInfo.IsDefined(typeof(IndexAttribute), false);
                var isArray = propInfo.PropertyType.IsArray;
                var propPype = propInfo.PropertyType.IsArray
                    ? propInfo.PropertyType.GetElementType()
                    : propInfo.PropertyType;
                var arraySize = 0;
                if (isArray)
                    arraySize = propInfo.GetCustomAttribute<CardinalityAttribute>().SizeConst;

                var dataRow = new DataGridViewRow();
                dataRow.Cells.Add(new DataGridViewTextBoxCell { Value = propInfo.Name });
                dataRow.Cells.Add(new DataGridViewTextBoxCell { Value = propPype.ToString() });
                dataRow.Cells.Add(new DataGridViewTextBoxCell { Value = arraySize == 0 ? "" : arraySize.ToString() });
                dataRow.Cells.Add(new DataGridViewCheckBoxCell { Value = isIndex });
                gridView.Rows.Add(dataRow);
            }
            gridView.Refresh();
        }

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
            _sourceStream?.Dispose();
            _targetStream?.Dispose();
        }
    }
}
