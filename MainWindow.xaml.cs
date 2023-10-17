using Meter_Attributes_Editor.Views;

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Meter_Attributes_Editor
{
    public partial class MainWindow : Window
    {
        public static AddMeterView? _addMeterView;
        public static EditMeterView? _editMeterView;
        public static DeleteMeterView? _deleteMeterView;

        private const string MeterAttributesFilePath = "C:\\Hospital Meter ATS\\Configurations\\MeterConfigurations.xml";

        private const string Version = "0.3";
        private const string Author = "Zachary Johnson & Anthony Meng-Lim";
        private const string BuildDate = "10/17/2023";

        private string saveLocation = "";

        private const Visibility Visible = Visibility.Visible;
        private const Visibility Hidden = Visibility.Hidden;
        private const Visibility Collapsed = Visibility.Collapsed;

        public bool output = false;


        public MainWindow()
        {
            InitializeComponent();

            createBackup.Visibility = Collapsed;
            saveFile.Visibility = Collapsed;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            mainMenuView.Visibility = Collapsed;
            addMeterView.Visibility = Visible;
            createBackup.Visibility = Visible;
            saveFile.Visibility = Visible;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            mainMenuView.Visibility = Collapsed;
            editMeterView.Visibility = Visible;
            saveFile.Visibility = Visible;
            createBackup.Visibility = Visible;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            mainMenuView.Visibility = Collapsed;
            deleteMeterView.Visibility = Visible;
            //saveFile.Visibility = Visible;
            //createBackup.Visibility = Visible;
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (createBackup.IsChecked == true)
            {
                string now = DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss");

                string backupFilePath = MeterAttributesFilePath.Replace(".xml", "_") + now + ".xml";

                try
                {
                    File.Copy(MeterAttributesFilePath, backupFilePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error creating backup file: " + ex.Message);
                }
            }

            if (addMeterView.Visibility == Visible)
            {
                try
                {
                    output = AddMeterView.WriteToXML();

                    if (output == true)
                    {
                        MessageBox.Show("New meter entry has been saved. Entry saved to C:\\Hospital Meter ATS\\Configurations\\MeterConfigurations.xml", "Meter Entry Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("There was an error trying to save the file. Error: " + ex.Message + " Contact Anthony Meng-Lim to assist.", "Error Saving", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (editMeterView.Visibility == Visible)
            {
                try
                {
                    output = editMeterView.SaveEditedMeter();
                    if (output == true)
                    {
                        MessageBox.Show("New meter entry has been saved. Entry saved to C:\\Hospital Meter ATS\\Configurations\\MeterConfigurations.xml", "Meter Entry Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error trying to save the file. Error: " + ex.Message + " Contact Anthony Meng-Lim to assist.", "Error Saving", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuResetEditor_Click(object sender, RoutedEventArgs e)
        {
            mainMenuView.Visibility = Visible;
            addMeterView.Visibility = Collapsed;
            editMeterView.Visibility = Collapsed;
            deleteMeterView.Visibility = Collapsed;
            saveFile.Visibility = Collapsed;
            createBackup.Visibility = Collapsed;

            _addMeterView?.Reset();
            _editMeterView?.Reset();
            _deleteMeterView?.Reset();

            _addMeterView.ConfigureElements();
        }

        private void MenuSaveReport_Click(object sender, RoutedEventArgs e)
        {
            string now = DateTime.Now.ToString("dd-M-yyyy-HH-mm-ss");
            string fileName = "report_" + now + ".png";
            //saveLocation = ReportsFolder + fileName;
        }

        private void MenuVersion_Click(object sender, RoutedEventArgs e)
        {
            string versionInfo = "Version: " + Version + "\n" +
                                 "Author: " + Author + "\n" +
                                 "Build Date: " + BuildDate;

            MessageBox.Show(versionInfo, "Version Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
