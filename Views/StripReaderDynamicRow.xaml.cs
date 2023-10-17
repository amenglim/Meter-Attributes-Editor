using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using Meter_Attributes_Editor.Views;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System;

namespace Meter_Attributes_Editor.Views
{
    public partial class StripReaderDynamicRow : UserControl
    {
        private readonly OpenFileDialog openFileDialog;
        public static AddMeterView? _addMeterView;

        public StripReaderDynamicRow()
        {
            InitializeComponent();

            openFileDialog = new();
        }

        private void OpenFileForStripReaderSoftware_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                stripReaderSoftwareFile.Text = openFileDialog.SafeFileName;
                // Find the last index of V.
                int findV = stripReaderSoftwareFile.Text.LastIndexOf('V');
                // Find the version number after character V
                int versionChar1 = findV + 1;
                int versionChar2 = findV + 4;

                stripReaderSoftwareFile.Text = stripReaderSoftwareFile.Text.Remove(versionChar1, 2).Insert(versionChar1, "??");
                stripReaderSoftwareFile.Text = stripReaderSoftwareFile.Text.Remove(versionChar2, 1).Insert(versionChar2, "?");

                _addMeterView.ConfigureElements();
            }
        }
        private void AddStripReaderRow_Click(object sender, RoutedEventArgs e)
        {
            AddMeterView.stripReaderRowCount++;
            //AddMeterView.ConfigureElements();

            if (AddMeterView.stripReaderRowCount < 3)
            {
                _addMeterView.addMeterView.RowDefinitions.Add(new RowDefinition());
                _addMeterView.CreateStripReaderRow();
                _addMeterView.ConfigureElements();
            }

        }

        private void stripReaderSoftwareType_TextChanged(object sender, TextChangedEventArgs e)
        {
            // XML doesn't allow number to be the first character.
            for(int i = 0; i < 10; i++)
            {
                string number = i.ToString();
                if (stripReaderSoftwareType.Text.StartsWith(number))
                {
                    stripReaderSoftwareType.Text = "";
                }

            }
        }
    }
}
