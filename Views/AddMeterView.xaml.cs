using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace Meter_Attributes_Editor.Views
{
    public partial class AddMeterView : UserControl
    {
        #region Global Variables
        public static AddMeterView? _AddMeterView = null;
        private readonly OpenFileDialog openFileDialog;
        private static readonly XmlDocument MeterAttributesFile = new();
        private static XmlNode? ConfigurationRecords;
        private static XmlElement? newMeter;
        public static int stripReaderRowCount;
        public static XmlNodeList meterIDList = null;
        private XmlNodeList? meterEntries;
        public static DeleteMeterView? _deleteMeterView;
        public static EditMeterView? _editMeterView;
        public static int saveCount;
        public static string meterCount;
        private const string MeterAttributesFilePath = "C:\\Hospital Meter ATS\\Configurations\\MeterConfigurations.xml";
        private readonly List<string> partNumbers = new();

        public static string[][] Elements =
            new string[][] {
                new string[2] { "PartNumber", "" },
                new string[2] { "Name", "" },
                new string[2] { "Description", "" },
                new string[2] { "Model", "" },
                new string[2] { "CountryCode", "" },
                new string[2] { "LanguageCode", "" },
                new string[2] { "DCSProductID", "" },
                new string[2] { "BoardIDCode", "" },
                new string[2] { "HostSoftwareFileFormat", "" },
                new string[2] { "StripReaderSoftware", ""},
                new string[2] { "Type", ""},
                new string[2] { "ID", ""},
                new string[2] { "File", ""},
            };
        #endregion

        #region AddMeterView Initialization
        public AddMeterView()
        {
            InitializeComponent();

            // Create the first strip reader row before the user inputs more.
            CreateStripReaderRow();

            MeterAttributesFile.Load(MeterAttributesFilePath);

            ConfigurationRecords = MeterAttributesFile.SelectSingleNode("/MeterConfigurations");

            openFileDialog = new();

            MainWindow._addMeterView = this;

            StripReaderDynamicRow._addMeterView = this;
        }
        #endregion

        #region CreateStripReaderRow
        // Public function to create multiple strip reader rows. Can support up to 2 button clicks.
        // Assigns a name for the created row, sets the row location, and names each component of the row. 
        // Adds the row to the children property of the addMeterView grid which will be used a lot.

        public void CreateStripReaderRow()
        {
            StripReaderDynamicRow dynamicRow;
            int rowNum = addMeterView.RowDefinitions.Count - 1;
            string stripReaderRowName = "StripReaderRow_" + rowNum;
            dynamicRow = new StripReaderDynamicRow
            {
                Name = stripReaderRowName
            };

            Grid.SetRow(dynamicRow, rowNum);
            Grid.SetColumnSpan(dynamicRow, 2);
            NameComponents(rowNum, dynamicRow);
            addMeterView.Children.Add(dynamicRow);

            if (rowNum == 7)
            {
                dynamicRow.newStripReaderRow.IsEnabled = false;
            }
            // If new strip row button is pressed, disable the last button.
            if (rowNum != 5)
            {
                // addMeterView has 15 children after first strip row is created. Disable the previous button by rowNum + 9.
                dynamicRow = addMeterView.Children[rowNum + 9] as StripReaderDynamicRow;
                dynamicRow.newStripReaderRow.IsEnabled = false;
            }

            var converter = new BrushConverter();
            if (stripReaderRowCount == 1)
            {
                dynamicRow = addMeterView.Children[15] as StripReaderDynamicRow;
                dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("LightGoldenrodYellow");
                dynamicRow = addMeterView.Children[16] as StripReaderDynamicRow;
                dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("#FFDBF5FD");
            }
            else if (stripReaderRowCount == 2)
            {
                dynamicRow = addMeterView.Children[15] as StripReaderDynamicRow;
                dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("LightGoldenrodYellow");
                dynamicRow = addMeterView.Children[16] as StripReaderDynamicRow;
                dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("#FFDBF5FD");
                dynamicRow = addMeterView.Children[17] as StripReaderDynamicRow;
                dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("#FFCDFFE7");
            }

        }
        #endregion

        #region NameComponents
        // Method to name each component in the different rows to be used for their specific data.
        private void NameComponents(int rowIndex, StripReaderDynamicRow row)
        {
            row.stripReaderSoftwareType.Name = "stripReaderSoftwareType_" + rowIndex;
            row.stripReaderSoftwareID.Name = "stripReaderSoftwareID_" + rowIndex;
            row.stripReaderSoftwareFile.Name = "stripReaderSoftwareFile_" + rowIndex;
            row.openFileForStripReaderSoftware.Name = "openFileForStripReaderSoftware_" + rowIndex;
        }
        #endregion

        #region Reset/Reload
        public void Reset()
        {
            MeterAttributesFile.Load(MeterAttributesFilePath);

            ConfigurationRecords = MeterAttributesFile.SelectSingleNode("/MeterConfigurations");

            InitializePartNumberSelector();

            foreach (string[] element in Elements)
            {
                element[1] = "";
            }

            partNumber.Text = "";
            meterName.Text = "";
            modelID.Text = "";
            modelDescription.Text = "";
            countryCode.Text = "";
            languageCode.Text = "";
            dcsProductionID.Text = "";
            boardIDCode.Text = "";
            hostSoftwareFileFormat.Text = "";

            // Keep the first strip reader row. 
            for (int i = 0; i <= stripReaderRowCount; i++)
            {
                if (stripReaderRowCount != 0)
                {
                    addMeterView.RowDefinitions.RemoveAt(stripReaderRowCount + 4);
                    addMeterView.Children.RemoveAt(stripReaderRowCount + 15);
                }
                stripReaderRowCount--;
            }
            stripReaderRowCount = 0;

            // Enable the first row strip reader button on reset. addMeterView has 9 children initially. 
            StripReaderDynamicRow dynamicRow = addMeterView.Children[15] as StripReaderDynamicRow;
            dynamicRow.stripReaderSoftwareFile.Text = "";
            dynamicRow.newStripReaderRow.IsEnabled = true;

            // Reset the elements list to only support up to 1 strip reader row. 
            Elements = new string[][] {
                new string[2] { "PartNumber", "" },
                new string[2] { "Name", "" },
                new string[2] { "Description", "" },
                new string[2] { "Model", "" },
                new string[2] { "CountryCode", "" },
                new string[2] { "LanguageCode", "" },
                new string[2] { "DCSProductID", "" },
                new string[2] { "BoardIDCode", "" },
                new string[2] { "HostSoftwareFileFormat", "" },
                new string[2] { "StripReaderSoftware", ""},
                new string[2] { "Type", "" },
                new string[2] { "ID", ""},
                new string[2] { "File", ""},
            };
        }

        private void InitializePartNumberSelector()
        {
            LoadMeterEntries();
        }

        private void LoadMeterEntries()
        {
            meterEntries = ConfigurationRecords.SelectNodes("/MeterConfigurations/Meter");

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

        #region WriteToXML
        // This function is called when the save button is clicked and the add meter view is enabled.
        public static bool WriteToXML()
        {
            newMeter = CreateNewMeterEntry();

            // Check if part number is empty when save button is clicked.
            if (Elements[0][1] == "")
            {
                MessageBox.Show("Enter a valid part number before clicking Save.", "Enter Valid Part Number", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            // Check if Strip Reader SW Type is empty when save button is clicked b/c this is a node in the xml.
            if (Elements[10][1] == "")
            {
                MessageBox.Show("Enter a valid Strip Reader Software Type before clicking Save.", "Enter Valid Strip Reader Software Type", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            ConfigurationRecords?.AppendChild(newMeter);
            MeterAttributesFile.Save(MeterAttributesFilePath);
            return true;
        }
        #endregion

        #region CreateNewMeterEntry
        // This function writes to the xml file
        private static XmlElement CreateNewMeterEntry()
        {
            XmlNode newMeterAttribute;
            newMeter = MeterAttributesFile.CreateElement("Meter");

            XmlElement root = MeterAttributesFile.DocumentElement;
            meterIDList = root.GetElementsByTagName("Meter");
            XmlAttribute meterID = MeterAttributesFile.CreateAttribute("id");

            // If a meter entry has been deleted in the DeleteMeterView OR,
            // If the save button has been clicked more than once in the AddMeter View,
            // Then reload the xml file and grab the latest meter count. 
            if (DeleteMeterView.deletedCount > 0 || saveCount > 0)
            {
                MeterAttributesFile.Load(MeterAttributesFilePath);
                ConfigurationRecords = MeterAttributesFile.SelectSingleNode("/MeterConfigurations");
                root = MeterAttributesFile.DocumentElement;
                meterIDList = root.GetElementsByTagName("Meter");
                meterID = MeterAttributesFile.CreateAttribute("id");
                meterID.Value = (meterIDList.Count + 1).ToString();
            }
            else
            {
                meterID.Value = (meterIDList.Count + 1).ToString();
            }
            // Set the latest meter id count with its appropriate meter ID
            newMeter.SetAttributeNode(meterID);
            saveCount++;

            // These nodes will be used to keep track of the parent and child node that is needed. 
            XmlNode stripReaderSoftwareNode = null;
            XmlNode stripReaderSoftwareIDNode = null;
            XmlNode stripReaderPreviousNode = null;
            XmlNode stripIDNode1 = null;
            XmlNode stripIDNode2 = null;
            XmlNode stripIDNode3 = null;
            int nodeCount = 0;
            // This function will set all of the chosen user input values to their corresponding Elements 
            MainWindow._addMeterView.Set();

            foreach (string[] element in Elements)
            {
                newMeterAttribute = MeterAttributesFile.CreateElement(element[0]);
                string stripReaderSoftwareFile = Elements[12][1];

                // The child nodes start to get complicated after StripReaderSoftware. Need to format the xml properly. 
                if (element[0] == "StripReaderSoftware" || element[0] == "ID" || element[0] == "File" || element[0] == "Type")
                {
                    // Skip Type because I already account for it. 
                    if (element[0] != "Type")
                    {
                        newMeter.AppendChild(newMeterAttribute);
                    }

                    if (element[0] == "StripReaderSoftware")
                    {
                        int elementCount = 0;
                        for (int i = 0; i < stripReaderRowCount + 1; i++)
                        {
                            string stripReaderSoftwareItem = Elements[elementCount + 10][1];
                            if (stripReaderSoftwareItem.Contains(" "))
                            {
                                stripReaderSoftwareItem = stripReaderSoftwareItem.Replace(" ", "");
                            }
                            if (stripReaderSoftwareItem.Contains("."))
                            {
                                stripReaderSoftwareItem = stripReaderSoftwareItem.Replace(".", "p");
                            }
                            // Gets the last child for the newMeter node (StripReaderSoftware).
                            stripReaderSoftwareNode = newMeter.LastChild;
                            newMeterAttribute = MeterAttributesFile.CreateElement(stripReaderSoftwareItem);
                            stripReaderSoftwareNode.AppendChild(newMeterAttribute);
                            // If there are multiple StripReaderRows, assign the ID and File to the right Type node by incrementing 3. 
                            if (elementCount == 0)
                            {
                                stripIDNode1 = stripReaderSoftwareNode.LastChild;
                            }
                            else if (elementCount == 3)
                            {
                                stripIDNode2 = stripReaderSoftwareNode.LastChild;
                            }
                            else if (elementCount == 6)
                            {
                                stripIDNode3 = stripReaderSoftwareNode.LastChild;
                            }
                            // Need elements 10, 13, 16 for Strip ID
                            elementCount += 3;

                        }
                    }
                    else if (element[0] == "ID")
                    {
                        if (element[1] != "")
                        {
                            // This is the content inside of <ID> <ID>. Need 20 characters (fill with spaces).
                            newMeterAttribute.InnerText = element[1].PadRight(20, ' ');
                        }
                        // Gets the last child for the StripReaderSoftware node (StripReader ID type). 
                        // Sets the ID to the proper parent node. 
                        if (nodeCount < 1)
                        {
                            stripReaderSoftwareIDNode = stripIDNode1;
                            stripReaderSoftwareIDNode.AppendChild(newMeterAttribute);
                            nodeCount++;
                        }
                        else if (nodeCount == 1)
                        {
                            stripReaderSoftwareIDNode = stripIDNode2;
                            stripReaderSoftwareIDNode.AppendChild(newMeterAttribute);
                            nodeCount++;
                        }
                        else if (nodeCount == 2)
                        {
                            stripReaderSoftwareIDNode = stripIDNode3;
                            stripReaderSoftwareIDNode.AppendChild(newMeterAttribute);
                            nodeCount++;
                        }

                    }
                    //Sets the File to the last node that was appended to. 
                    else if (element[0] == "File")
                    {
                        if (element[1] != "")
                        {
                            newMeterAttribute.InnerText = element[1];
                        }
                        XmlNode stripReaderSoftwareFileNode = stripReaderSoftwareIDNode;
                        stripReaderSoftwareFileNode.AppendChild(newMeterAttribute);
                    }
                }
                else
                {
                    if (element[1] != "")
                    {
                        newMeterAttribute.InnerText = element[1];
                    }
                    newMeter.AppendChild(newMeterAttribute);
                }
            }
            return newMeter;
        }
        #endregion

        #region Set
        // This function will set all of the chosen user input values to their corresponding Elements in their proper row.
        // It also keeps track of the amount of strip reader rows are enabled which will change how many Elements are needed. 
        public void Set()
        {
            StripReaderDynamicRow currentRow = addMeterView.Children[15] as StripReaderDynamicRow;
            if (stripReaderRowCount == 1)
            {
                Elements[10][1] = currentRow.stripReaderSoftwareType.Text;
                Elements[11][1] = currentRow.stripReaderSoftwareID.Text;
                Elements[12][1] = currentRow.stripReaderSoftwareFile.Text;
                currentRow = addMeterView.Children[16] as StripReaderDynamicRow;
                Elements[13][1] = currentRow.stripReaderSoftwareType.Text;
                Elements[14][1] = currentRow.stripReaderSoftwareID.Text;
                Elements[15][1] = currentRow.stripReaderSoftwareFile.Text;
            }
            else if (stripReaderRowCount == 2)
            {
                Elements[10][1] = currentRow.stripReaderSoftwareType.Text;
                Elements[11][1] = currentRow.stripReaderSoftwareID.Text;
                Elements[12][1] = currentRow.stripReaderSoftwareFile.Text;
                currentRow = addMeterView.Children[16] as StripReaderDynamicRow;
                Elements[13][1] = currentRow.stripReaderSoftwareType.Text;
                Elements[14][1] = currentRow.stripReaderSoftwareID.Text;
                Elements[15][1] = currentRow.stripReaderSoftwareFile.Text;
                currentRow = addMeterView.Children[17] as StripReaderDynamicRow;
                Elements[16][1] = currentRow.stripReaderSoftwareType.Text;
                Elements[17][1] = currentRow.stripReaderSoftwareID.Text;
                Elements[18][1] = currentRow.stripReaderSoftwareFile.Text;
            }
            else
            {
                Elements[10][1] = currentRow.stripReaderSoftwareType.Text;
                Elements[11][1] = currentRow.stripReaderSoftwareID.Text;
                Elements[12][1] = currentRow.stripReaderSoftwareFile.Text;
            }
            Elements[0][1] = partNumber.Text;
            Elements[1][1] = meterName.Text;
            Elements[2][1] = modelDescription.Text;
            Elements[3][1] = modelID.Text;
            Elements[4][1] = countryCode.Text;
            Elements[5][1] = languageCode.Text;
            Elements[6][1] = dcsProductionID.Text;
            Elements[7][1] = boardIDCode.Text;
            Elements[8][1] = hostSoftwareFileFormat.Text;
        }
        #endregion

        #region ConfigureElements

        // Depending on the amount of strip reader rows the user chooses, configure the Elements list based on that count. 
        public void ConfigureElements()
        {
            if (stripReaderRowCount == 1)
            {
                Elements = new string[][] {
                    new string[2] { "PartNumber", "" },
                    new string[2] { "Name", "" },
                    new string[2] { "Description", "" },
                    new string[2] { "Model", "" },
                    new string[2] { "CountryCode", "" },
                    new string[2] { "LanguageCode", "" },
                    new string[2] { "DCSProductID", "" },
                    new string[2] { "BoardIDCode", "" },
                    new string[2] { "HostSoftwareFileFormat", "" },
                    new string[2] { "StripReaderSoftware", ""},
                    new string[2] { "Type", ""},
                    new string[2] { "ID", ""},
                    new string[2] { "File", ""},
                    new string[2] { "Type", ""},
                    new string[2] { "ID", "" },
                    new string[2] { "File", ""}
                };
            }
            else if (stripReaderRowCount == 2)
            {
                Elements = new string[][] {
                    new string[2] { "PartNumber", "" },
                    new string[2] { "Name", "" },
                    new string[2] { "Description", "" },
                    new string[2] { "Model", "" },
                    new string[2] { "CountryCode", "" },
                    new string[2] { "LanguageCode", "" },
                    new string[2] { "DCSProductID", "" },
                    new string[2] { "BoardIDCode", "" },
                    new string[2] { "HostSoftwareFileFormat", "" },
                    new string[2] { "StripReaderSoftware", ""},
                    new string[2] { "Type", ""},
                    new string[2] { "ID", ""},
                    new string[2] { "File", ""},
                    new string[2] { "Type", ""},
                    new string[2] { "ID", "" },
                    new string[2] { "File", ""},
                    new string[2] { "Type", ""},
                    new string[2] { "ID", "" },
                    new string[2] { "File", ""}
                };
            }
        }
        #endregion

        #region User Input Methods
        private void PartNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            Elements[0][1] = partNumber.Text;
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Disable letters input in Textbox and only accept numbers.
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void MeterName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Elements[1][1] = meterName.Text;
        }
        private void ModelDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            Elements[2][1] = modelDescription.Text;
        }

        private void ModelID_TextChanged(object sender, TextChangedEventArgs e)
        {
            Elements[3][1] = modelID.Text;
        }

        private void CountryCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            Elements[4][1] = countryCode.Text;
        }

        private void LanguageCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            Elements[5][1] = languageCode.Text;
        }
        private void DcsProductionID_TextChanged(object sender, TextChangedEventArgs e)
        {
            Elements[6][1] = dcsProductionID.Text;
        }

        private void BoardIDCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            Elements[7][1] = boardIDCode.Text;
        }

        private void OpenFileHostSoftwareFileFormat_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                // This will replace the unnecessary values in Host SW files. (ex. StatSensor39i_4_4_8_32_en -> StatSensor39i_*_en)
                hostSoftwareFileFormat.Text = openFileDialog.SafeFileName;
                // Keep the first underscore.
                int firstUnderscore = hostSoftwareFileFormat.Text.IndexOf('_') + 1;
                int lastUnderscore = hostSoftwareFileFormat.Text.LastIndexOf('_');
                int charactersRemoved = lastUnderscore - firstUnderscore;

                try
                {
                    hostSoftwareFileFormat.Text = hostSoftwareFileFormat.Text.Remove(firstUnderscore, charactersRemoved).Insert(firstUnderscore, "*");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error trying to choose a host file. Error: " + ex.Message + " Choose a proper host file.", "Error Choosing Host File", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

            Elements[8][1] = hostSoftwareFileFormat.Text;
            
        }
        #endregion
    }

}