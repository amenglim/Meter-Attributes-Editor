using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace Meter_Attributes_Editor.Views
{
    public partial class DeleteMeterView : UserControl
    {
        #region Global Variables
        private static readonly XmlDocument MeterAttributesFile = new();
        public List<string> partNumbers = new();
        private XmlNodeList? meterEntries;
        private XmlNode? meter;

        private const string MeterAttributesFilePath = "C:\\Hospital Meter ATS\\Configurations\\MeterConfigurations.xml";
        private string? meterXPath;
        private static XmlNode? ConfigurationRecords;
        public static int deletedCount;
        #endregion

        #region DeleteMeterView Initialization
        public DeleteMeterView()
        {
            InitializeComponent();

            DataContext = this;

            MeterAttributesFile.Load(MeterAttributesFilePath);

            ConfigurationRecords = MeterAttributesFile.SelectSingleNode("/MeterConfigurations");

            InitializePartNumberSelector();

            MainWindow._deleteMeterView = this;

            AddMeterView._deleteMeterView = this;
        }
        #endregion

        #region Reset/Reload
        public void Reset()
        {
            ConfigurationRecords = MeterAttributesFile.SelectSingleNode("/MeterConfigurations");

            InitializePartNumberSelector();
            ReInitializePartNumberSelector();

            partNumberSelector.SelectedIndex = -1;
        }

        private void InitializePartNumberSelector()
        {
            LoadMeterEntries();
            partNumberSelector.ItemsSource = partNumbers;
        }

        private void ReInitializePartNumberSelector()
        {
            partNumbers.Clear();
            LoadMeterEntries();
            partNumberSelector.Items.Refresh();
            partNumberSelector.SelectedIndex = -1;
        }

        // This function selects all the "Meter" nodes and adds them to the partNumber list. 
        private void LoadMeterEntries()
        {
            meterEntries = MeterAttributesFile.SelectNodes("/MeterConfigurations/Meter");

            // Reload the xml file each time reset is pressed. 
            MeterAttributesFile.Load(MeterAttributesFilePath);
            ConfigurationRecords = MeterAttributesFile.SelectSingleNode("/MeterConfigurations");

            // When reset is clicked, refresh the list by clearing it.
            if (partNumbers.Count > 0)
            {
                partNumbers.Clear();
            }

            foreach (XmlNode meter in meterEntries)
            {
                partNumbers.Add(meter.FirstChild.InnerText);
            }
        }
        #endregion

        #region User Input Methods
        private void PartNumberSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (partNumberSelector.SelectedIndex != -1)
            {
                meterXPath = "/MeterConfigurations/Meter[PartNumber=" + partNumberSelector.SelectedItem.ToString() + "]";
                try
                {
                    meter = ConfigurationRecords.SelectSingleNode(meterXPath);
                }
                catch
                {
                    MessageBox.Show("Part number not found or may be null. Please edit another meter entry or make sure the part number is correct.", "Part Number Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteMeter_Click(object sender, RoutedEventArgs e)
        {
            if (meter != null)
            {
                // Remove the child node when delete button is clicked. 
                MeterAttributesFile.SelectSingleNode(meterXPath).ParentNode.RemoveChild(meter);

                MeterAttributesFile.Save(MeterAttributesFilePath);

                deletedCount++;

                ReInitializePartNumberSelector();

                MessageBox.Show("Meter entry has been deleted.", "Deleted Meter Entry", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No meter has been selected, please select a meter to proceed.", "No Meter Selected", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}