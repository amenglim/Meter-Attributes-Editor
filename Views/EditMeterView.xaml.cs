using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace Meter_Attributes_Editor.Views
{
    public partial class EditMeterView : UserControl
    {
        #region Global Variables
        private readonly OpenFileDialog openFileDialog;

        private static readonly XmlDocument MeterAttributesFile = new();
        private static XmlNode? ConfigurationRecords;

        private const string MeterAttributesFilePath = "C:\\Hospital Meter ATS\\Configurations\\MeterConfigurations.xml";

        private XmlNodeList? meterEntries;
        private static XmlNode? meter;

        private readonly List<string> partNumbers = new();
        private string? meterXPath;

        public static int stripReaderRowCount;
        // These nodes will be used to keep track of the parent and child node that is needed. 
        public static XmlNode stripReaderSoftwareNode = null;
        public static XmlNode stripIDNode1;
        public static XmlNode stripIDNode2 = null;
        public static XmlNode stripIDNode3;


        XmlNode oldStripReaderSoftwareType = null;
        XmlNode newStripReaderSoftwareType = null;
        XmlNode editedMeterAttribute = null;

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

        #region EditMeterView Initialization
        public EditMeterView()
        {
            InitializeComponent();

            // Create the first strip reader row. 
            CreateStripReaderRow();

            DataContext = this;

            openFileDialog = new();

            MeterAttributesFile.Load(MeterAttributesFilePath);

            ConfigurationRecords = MeterAttributesFile.SelectSingleNode("/MeterConfigurations");

            InitializePartNumberSelector();

            selectMeterToEdit.Visibility = Visibility.Visible;
            editMeterGrid.Visibility = Visibility.Collapsed;

            MainWindow._editMeterView = this;
            AddMeterView._editMeterView = this;
        }
        #endregion

        #region Reset/Reload
        public void Reset()
        {
            MeterAttributesFile.Load(MeterAttributesFilePath);

            ConfigurationRecords = MeterAttributesFile.SelectSingleNode("/MeterConfigurations");

            InitializePartNumberSelector();
            ReInitializePartNumberSelector();

            selectMeterToEdit.Visibility = Visibility.Visible;
            editMeterGrid.Visibility = Visibility.Collapsed;

            partNumberSelector.SelectedIndex = -1;

            foreach (string[] element in Elements)
            {
                element[1] = "";
            }

            // Keep the first strip reader row.
            for (int i = 0; i <= stripReaderRowCount; i++)
            {
                if (stripReaderRowCount != 0)
                {
                    editMeterGrid.RowDefinitions.RemoveAt(stripReaderRowCount + 4);
                    editMeterGrid.Children.RemoveAt(stripReaderRowCount + 15);
                }
                stripReaderRowCount--;
            }

            stripReaderRowCount = 0;
            // Enable the first row strip reader button on reset. editMeterGrid has 9 children initially. 
            StripReaderDynamicRow dynamicRow = editMeterGrid.Children[15] as StripReaderDynamicRow;
            dynamicRow.stripReaderSoftwareFile.Text = "";
            dynamicRow.newStripReaderRow.IsEnabled = true;

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
            };

            partNumber.Text = "";
            meterName.Text = "";
            modelID.Text = "";
            modelDescription.Text = "";
            countryCode.Text = "";
            languageCode.Text = "";
            dcsProductionID.Text = "";
            boardIDCode.Text = "";
            hostSoftwareFileFormat.Text = "";
        }
        private void ReInitializePartNumberSelector()
        {
            partNumbers.Clear();
            LoadMeterEntries();
            partNumberSelector.Items.Refresh();
            partNumberSelector.SelectedIndex = -1;
        }

        private void InitializePartNumberSelector()
        {
            LoadMeterEntries();
            partNumberSelector.ItemsSource = partNumbers;
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

        #region SaveEditedMeter
        // This function is called when the Save button is clicked and the EditMeterView is visible.
        public bool SaveEditedMeter()
        {
            EditMeterEntry();

            // Check if part number is empty when save button is clicked.
            if (Elements[0][1] == "")
            {
                MessageBox.Show("Enter a valid part number before clicking Save.", "Enter Valid Part Number", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            MeterAttributesFile.Save(MeterAttributesFilePath);
            return true;
        }
        #endregion

        #region EditMeterEntry
        // Depending on the amount of strip reader rows the user inputs, this function
        // edits the row properties by recreating the StripReaderDynamicRow and appending the new values to the xml.
        public void EditMeterEntry()
        {
            int i = 0;
            string node1 = "index0";

            foreach (string[] element in Elements)
            {
                editedMeterAttribute = MeterAttributesFile.CreateElement(element[0]);

                if (element[1] != "")
                {
                    editedMeterAttribute.InnerText = element[1];
                }

                // The first strip reader row is child 9 of editMeterGrid.
                StripReaderDynamicRow currentRow = editMeterGrid.Children[15] as StripReaderDynamicRow;

                // Skip Type because I already account for it. 
                if (element[0] == "Type") { continue; }

                // One StripReader Row.
                if (stripReaderRowCount == 0)
                {
                    if (element[0] == "StripReaderSoftware" || element[0] == "ID" || element[0] == "File")
                    {
                        if (element[0] == "StripReaderSoftware")
                        {
                            AppendStripReaderSoftwareNode(currentRow);
                        }

                        if (element[0] == "ID")
                        {
                            AppendFirstStripReaderID(currentRow);
                        }
                        else if (element[0] == "File")
                        {
                            AppendFirstStripReaderFile(currentRow);
                        }
                    }
                    else
                    {
                        meter.ReplaceChild(editedMeterAttribute, meter.ChildNodes[i]);
                    }
                    i++;
                }
                // Two StripReader Rows. 
                else if (stripReaderRowCount == 1)
                {
                    if (element[0] == "StripReaderSoftware" || element[0] == "ID" || element[0] == "File" || element[0] == "Type")
                    {
                        if (element[0] == "StripReaderSoftware" || node1 == "index2")
                        {
                            if (node1 == "index0")
                            {
                                AppendStripReaderSoftwareNode(currentRow);
                                node1 = "index1";
                            }
                            else if (node1 == "index2")
                            {
                                AppendSecondStripReaderIDNode(currentRow);
                                node1 = "index3";
                            }
                        }
                        if (node1 == "index1" && element[0] == "ID")
                        {
                            AppendFirstStripReaderID(currentRow);
                        }
                        else if (node1 == "index1" && element[0] == "File")
                        {
                            AppendFirstStripReaderFile(currentRow);
                            node1 = "index2";
                        }

                        if (node1 == "index3" && element[0] == "ID")
                        {
                            AppendSecondStripReaderID(currentRow);
                        }
                        else if (node1 == "index3" && element[0] == "File")
                        {
                            AppendSecondStripReaderFile(currentRow);
                        }
                    }
                    else
                    {
                        meter.ReplaceChild(editedMeterAttribute, meter.ChildNodes[i]);
                    }
                    i++;
                }
                // Three StripReader Rows.
                else if (stripReaderRowCount == 2)
                {
                    if (element[0] == "StripReaderSoftware" || element[0] == "ID" || element[0] == "File" || element[0] == "Type")
                    {
                        if (element[0] == "StripReaderSoftware" || node1 == "index2" || node1 == "index4")
                        {
                            if (node1 == "index0")
                            {
                                AppendStripReaderSoftwareNode(currentRow);
                                node1 = "index1";
                            }
                            else if (node1 == "index2")
                            {
                                AppendSecondStripReaderIDNode(currentRow);
                                node1 = "index3";
                            }
                            else if (node1 == "index4")
                            {
                                AppendThirdStripReaderIDNode(currentRow);

                                node1 = "index5";
                            }
                        }
                        if (node1 == "index1" && element[0] == "ID")
                        {
                            AppendFirstStripReaderID(currentRow);
                        }
                        else if (node1 == "index1" && element[0] == "File")
                        {
                            AppendFirstStripReaderFile(currentRow);
                            node1 = "index2";
                        }

                        if (node1 == "index3" && element[0] == "ID")
                        {
                            AppendSecondStripReaderID(currentRow);
                        }
                        else if (node1 == "index3" && element[0] == "File")
                        {
                            AppendSecondStripReaderFile(currentRow);
                            node1 = "index4";
                        }

                        if (node1 == "index5" && element[0] == "ID")
                        {
                            AppendThirdStripReaderID(currentRow);
                        }
                        else if (node1 == "index5" && element[0] == "File")
                        {
                            AppendThirdStripReaderFile(currentRow);
                        }
                    }
                    else
                    {
                        meter.ReplaceChild(editedMeterAttribute, meter.ChildNodes[i]);
                    }
                    i++;
                }
            }
        }
        #endregion

        #region Append Methods for EditMeterEntry
        private void AppendStripReaderSoftwareNode(StripReaderDynamicRow row)
        {
            meter.AppendChild(stripReaderSoftwareNode);
            Elements[10][1] = row.stripReaderSoftwareType.Text;
            string stripReaderSoftwareItem = Elements[10][1];
            if (stripReaderSoftwareItem.Contains(" "))
            {
                stripReaderSoftwareItem = stripReaderSoftwareItem.Replace(" ", "");
            }
            if (stripReaderSoftwareItem.Contains("."))
            {
                stripReaderSoftwareItem = stripReaderSoftwareItem.Replace(".", "p");
            }
            oldStripReaderSoftwareType = stripReaderSoftwareNode.FirstChild;
            newStripReaderSoftwareType = MeterAttributesFile.CreateElement(stripReaderSoftwareItem);
            stripReaderSoftwareNode.ReplaceChild(newStripReaderSoftwareType, oldStripReaderSoftwareType);
            meter.AppendChild(stripReaderSoftwareNode);
        }

        private void AppendFirstStripReaderID(StripReaderDynamicRow row)
        {
            Elements[11][1] = row.stripReaderSoftwareID.Text;
            editedMeterAttribute.InnerText = Elements[11][1].PadRight(20, ' ');

            stripReaderSoftwareNode.FirstChild.AppendChild(editedMeterAttribute);
            meter.AppendChild(stripReaderSoftwareNode);
        }

        private void AppendFirstStripReaderFile(StripReaderDynamicRow row)
        {
            Elements[12][1] = row.stripReaderSoftwareFile.Text;
            if (editedMeterAttribute.InnerText != "")
            {
                editedMeterAttribute.InnerText = Elements[12][1];
            }

            stripReaderSoftwareNode.FirstChild.AppendChild(editedMeterAttribute);

            meter.AppendChild(stripReaderSoftwareNode);

        }

        private void AppendSecondStripReaderIDNode(StripReaderDynamicRow row)
        {
            row = editMeterGrid.Children[16] as StripReaderDynamicRow;
            Elements[13][1] = row.stripReaderSoftwareType.Text;
            string stripReaderSoftwareItem = Elements[13][1];
            if (stripReaderSoftwareItem.Contains(" "))
            {
                stripReaderSoftwareItem = stripReaderSoftwareItem.Replace(" ", "");
            }
            if (stripReaderSoftwareItem.Contains("."))
            {
                stripReaderSoftwareItem = stripReaderSoftwareItem.Replace(".", "p");
            }
            oldStripReaderSoftwareType = stripReaderSoftwareNode.ChildNodes[1];
            newStripReaderSoftwareType = MeterAttributesFile.CreateElement(stripReaderSoftwareItem);
            stripReaderSoftwareNode.ReplaceChild(newStripReaderSoftwareType, oldStripReaderSoftwareType);
            meter.AppendChild(stripReaderSoftwareNode);
        }

        private void AppendSecondStripReaderID(StripReaderDynamicRow row)
        {
            row = editMeterGrid.Children[16] as StripReaderDynamicRow;
            Elements[14][1] = row.stripReaderSoftwareID.Text;
            editedMeterAttribute.InnerText = Elements[14][1].PadRight(20, ' ');

            stripReaderSoftwareNode.ChildNodes[1].AppendChild(editedMeterAttribute);
            meter.AppendChild(stripReaderSoftwareNode);
        }

        private void AppendSecondStripReaderFile(StripReaderDynamicRow row)
        {
            row = editMeterGrid.Children[16] as StripReaderDynamicRow;
            Elements[15][1] = row.stripReaderSoftwareFile.Text;
            if (editedMeterAttribute.InnerText != "")
            {
                editedMeterAttribute.InnerText = Elements[15][1];
            }
            stripReaderSoftwareNode.ChildNodes[1].AppendChild(editedMeterAttribute);
            meter.AppendChild(stripReaderSoftwareNode);

        }

        private void AppendThirdStripReaderIDNode(StripReaderDynamicRow row)
        {
            row = editMeterGrid.Children[17] as StripReaderDynamicRow;
            Elements[16][1] = row.stripReaderSoftwareType.Text;
            string stripReaderSoftwareItem = Elements[16][1];
            if (stripReaderSoftwareItem.Contains(" "))
            {
                stripReaderSoftwareItem = stripReaderSoftwareItem.Replace(" ", "");
            }
            if (stripReaderSoftwareItem.Contains("."))
            {
                stripReaderSoftwareItem = stripReaderSoftwareItem.Replace(".", "p");
            }
            oldStripReaderSoftwareType = stripReaderSoftwareNode.ChildNodes[2];
            newStripReaderSoftwareType = MeterAttributesFile.CreateElement(stripReaderSoftwareItem);
            stripReaderSoftwareNode.ReplaceChild(newStripReaderSoftwareType, oldStripReaderSoftwareType);
            meter.AppendChild(stripReaderSoftwareNode);
        }

        private void AppendThirdStripReaderID(StripReaderDynamicRow row)
        {
            row = editMeterGrid.Children[17] as StripReaderDynamicRow;
            Elements[17][1] = row.stripReaderSoftwareID.Text;
            editedMeterAttribute.InnerText = Elements[17][1].PadRight(20, ' ');

            stripReaderSoftwareNode.ChildNodes[2].AppendChild(editedMeterAttribute);
            meter.AppendChild(stripReaderSoftwareNode);
        }

        private void AppendThirdStripReaderFile(StripReaderDynamicRow row)
        {
            row = editMeterGrid.Children[17] as StripReaderDynamicRow;
            Elements[18][1] = row.stripReaderSoftwareFile.Text;
            if (editedMeterAttribute.InnerText != "")
            {
                editedMeterAttribute.InnerText = Elements[18][1];
            }
            stripReaderSoftwareNode.ChildNodes[2].AppendChild(editedMeterAttribute);
            meter.AppendChild(stripReaderSoftwareNode);
        }

        #endregion

        #region FillFieldsToEdit
        private void FillFieldsToEdit(XmlNodeList fields)
        {
            partNumber.Text = fields[0].InnerText;
            meterName.Text = fields[1].InnerText;
            modelDescription.Text = fields[2].InnerText;
            modelID.Text = fields[3].InnerText;
            countryCode.Text = fields[4].InnerText;
            languageCode.Text = fields[5].InnerText;
            dcsProductionID.Text = fields[6].InnerText;
            boardIDCode.Text = fields[7].InnerText;
            hostSoftwareFileFormat.Text = fields[8].InnerText;

            GenerateStripReaderFieldInformation();

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

        private void GenerateStripReaderFieldInformation()
        {
            StripReaderDynamicRow currentRow = editMeterGrid.Children[15] as StripReaderDynamicRow;

            if (stripReaderRowCount == 1)
            {
                GenerateFirstStripReaderRow(currentRow);

                currentRow = editMeterGrid.Children[16] as StripReaderDynamicRow;

                GenerateSecondStripReaderRow(currentRow);
            }
            else if (stripReaderRowCount == 2)
            {
                GenerateFirstStripReaderRow(currentRow);

                currentRow = editMeterGrid.Children[16] as StripReaderDynamicRow;

                GenerateSecondStripReaderRow(currentRow);

                currentRow = editMeterGrid.Children[17] as StripReaderDynamicRow;

                GenerateThirdStripReaderRow(currentRow);
            }
            else
            {
                GenerateFirstStripReaderRow(currentRow);
            }
        }

        private void GenerateFirstStripReaderRow(StripReaderDynamicRow row)
        {
            stripIDNode1 = stripReaderSoftwareNode.FirstChild;
            Elements[10][1] = stripReaderSoftwareNode.FirstChild.Name;
            Elements[11][1] = stripIDNode1.FirstChild.InnerText;
            Elements[12][1] = stripIDNode1.LastChild.InnerText;           

            row.stripReaderSoftwareType.Text = Elements[10][1];
            row.stripReaderSoftwareID.Text = Elements[11][1].Trim(' ');
            row.stripReaderSoftwareFile.SelectedText = Elements[12][1];
        }

        private void GenerateSecondStripReaderRow(StripReaderDynamicRow row)
        {
            stripIDNode2 = stripReaderSoftwareNode.ChildNodes[1];
            Elements[13][1] = stripReaderSoftwareNode.ChildNodes[1].Name;
            Elements[14][1] = stripIDNode2.FirstChild.InnerText;
            Elements[15][1] = stripIDNode2.LastChild.InnerText;

            row.stripReaderSoftwareType.Text = Elements[13][1];
            row.stripReaderSoftwareID.Text = Elements[14][1].Trim(' ');
            row.stripReaderSoftwareFile.SelectedText = stripIDNode2.LastChild.InnerText;
        }

        private void GenerateThirdStripReaderRow(StripReaderDynamicRow row)
        {
            stripIDNode3 = stripReaderSoftwareNode.ChildNodes[2];
            Elements[16][1] = stripReaderSoftwareNode.ChildNodes[2].Name;
            Elements[17][1] = stripIDNode3.FirstChild.InnerText;
            Elements[18][1] = stripIDNode3.LastChild.InnerText;

            row.stripReaderSoftwareType.Text = Elements[16][1];
            row.stripReaderSoftwareID.Text = Elements[17][1].Trim(' ');
            row.stripReaderSoftwareFile.SelectedText = stripIDNode3.LastChild.InnerText;
        }
        #endregion

        #region SelectMeter & CreateStripReaderRow
        // This function configures the EditMeterGrid and adds addtional StripReaderRows depending on 
        // which meter the user selects and how many rows were in those entries. Then the fields are filled 
        // and the user can edit those fields as they please. 
        private void SelectMeter_Click(object sender, RoutedEventArgs e)
        {
            if (meter != null)
            {
                var converter = new BrushConverter();

                stripReaderSoftwareNode = meter.LastChild;
                if (stripReaderSoftwareNode.ChildNodes.Count == 2)
                {
                    stripReaderRowCount = stripReaderSoftwareNode.ChildNodes.Count - 1;
                    editMeterGrid.RowDefinitions.Add(new RowDefinition());
                    CreateStripReaderRow();
                    StripReaderDynamicRow dynamicRow = editMeterGrid.Children[15] as StripReaderDynamicRow;
                    dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("LightGoldenrodYellow");
                    dynamicRow = editMeterGrid.Children[16] as StripReaderDynamicRow;
                    dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("#FFDBF5FD");
                    ConfigureElements();
                }
                else if (stripReaderSoftwareNode.ChildNodes.Count == 3)
                {
                    stripReaderRowCount = stripReaderSoftwareNode.ChildNodes.Count - 1;
                    editMeterGrid.RowDefinitions.Add(new RowDefinition());
                    CreateStripReaderRow();
                    ConfigureElements();

                    editMeterGrid.RowDefinitions.Add(new RowDefinition());
                    CreateStripReaderRow();
                    ConfigureElements();

                    StripReaderDynamicRow dynamicRow = editMeterGrid.Children[15] as StripReaderDynamicRow;
                    dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("LightGoldenrodYellow");
                    dynamicRow = editMeterGrid.Children[16] as StripReaderDynamicRow;
                    dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("#FFDBF5FD");
                    dynamicRow = editMeterGrid.Children[17] as StripReaderDynamicRow;
                    dynamicRow.backgroundColor.Background = (Brush)converter.ConvertFromString("#FFCDFFE7");
                }

                FillFieldsToEdit(meter.ChildNodes);

                selectMeterToEdit.Visibility = Visibility.Collapsed;
                editMeterGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("No meter has been selected, please select a meter to proceed.", "No Meter Selected");
            }
        }

        // Public function to create multiple strip reader rows
        // Assigns a name for the created row, sets the row location, and names each component of the row. 
        // Adds the row to the children property of the editMeterGrid which will be used a lot.
        public void CreateStripReaderRow()
        {
            StripReaderDynamicRow dynamicRow;
            int rowNum = editMeterGrid.RowDefinitions.Count - 1;
            string stripReaderRowName = "StripReaderRow_" + rowNum;
            dynamicRow = new StripReaderDynamicRow
            {
                Name = stripReaderRowName
            };

            dynamicRow.newStripReaderRow.IsEnabled = false;

            Grid.SetRow(dynamicRow, rowNum);
            Grid.SetColumnSpan(dynamicRow, 2);
            NameComponents(rowNum, dynamicRow);
            editMeterGrid.Children.Add(dynamicRow);

            if (rowNum == 7)
            {
                dynamicRow.newStripReaderRow.IsEnabled = false;
            }

            // If new strip row button is pressed, disable the last button
            if (rowNum != 5)
            {
                // editMeterGrid has 15 children after first strip row is created. Disable the previous button by rowNum + 9.
                dynamicRow = editMeterGrid.Children[rowNum + 9] as StripReaderDynamicRow;
                dynamicRow.newStripReaderRow.IsEnabled = false;

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

        private void PartNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            Elements[0][1] = partNumber.Text;
        }

        private void NumberValidationTextBox(object sender, System.Windows.Input.TextCompositionEventArgs e)
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

                Elements[8][1] = hostSoftwareFileFormat.Text;
            }
        }
        #endregion

    }
}

