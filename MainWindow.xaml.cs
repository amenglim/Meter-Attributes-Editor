using Meter_Attributes_Editor.Views;

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Meter_Attributes_Editor
{
    public partial class MainWindow : Window
    {
        public static AddMeterView? _addMeterView;
        public static EditMeterView? _editMeterView;
        public static DeleteMeterView? _deleteMeterView;

        private const string MeterAttributesFilePath = "C:\\Hospital Meter ATS\\Configurations\\MeterConfigurations.xml";

        private const string Version = "0.3";
        private const string Author = "Anthony Meng-Lim";
        private const string BuildDate = "10/23/2023";

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
                catch (Exception ex)
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

        private void submitPassword_Clicked(object sender, RoutedEventArgs e)
        {
            if (passwordBox.Password == "Nova1000" || passwordShow.Text == "Nova1000")
            {
                mainMenuView.Visibility = Visibility.Visible;
                mainMenuBottomView.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Collapsed;
                passwordPrompt.Visibility = Visibility.Collapsed;
                submitPassword.Visibility = Visibility.Collapsed;
                eyehidden.Visibility = Visibility.Collapsed;
                eyeshow.Visibility = Visibility.Collapsed;
                passwordShow.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("Incorrect password, please try again.", "Incorrect Password", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.CapsLock) & KeyStates.Toggled) == KeyStates.Toggled)
            {
                if(passwordBox.ToolTip == null)
                {
                    ToolTip capslockWarning = new ToolTip();
                    capslockWarning.Content = "Warning: CapsLock is on";
                    capslockWarning.FontSize = 20;
                    capslockWarning.Foreground = Brushes.Red;
                    capslockWarning.FontWeight = FontWeights.Bold;
                    capslockWarning.PlacementTarget = sender as UIElement;
                    capslockWarning.Placement = PlacementMode.Bottom;
                    passwordBox.ToolTip = capslockWarning;
                    capslockWarning.IsOpen = true;
                }

            }
            else
            {
                var currentToolTip = passwordBox.ToolTip as ToolTip;
                if (currentToolTip != null)
                {
                    currentToolTip.IsOpen = false;
                }

                passwordBox.ToolTip = null;
            }
        }

        private void eyeshow_Click(object sender, RoutedEventArgs e)
        {
            eyeshow.Visibility = Visibility.Collapsed;
            eyehidden.Visibility = Visibility.Visible;
            passwordShow.Visibility = Visibility.Collapsed;
            passwordBox.Password = passwordShow.Text;
            passwordBox.PasswordChar = '*';

        }

        private void eyehidden_Click(object sender, RoutedEventArgs e)
        {
            eyeshow.Visibility = Visibility.Visible;
            eyehidden.Visibility = Visibility.Collapsed;
            passwordShow.Visibility = Visibility.Visible;
            passwordShow.Text = passwordBox.Password;
            //passwordBox.PasswordChar = '\0';
        }
    }
}
