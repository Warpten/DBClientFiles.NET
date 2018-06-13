using System;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DBClientFiles.NET.Mapper.UI.Forms
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void BrowseDirectory(object sender, EventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = "Navigate to the folder containing DBD definitions.";
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    textBox1.Text = dialog.FileName;
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.DefinitionRoot = textBox1.Text;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.DefinitionRoot;
            checkBox1.Checked = Properties.Settings.Default.LoadTargetDefinition;
        }
    }
}
