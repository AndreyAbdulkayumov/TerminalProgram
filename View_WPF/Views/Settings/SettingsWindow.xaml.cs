using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup.Localizer;
using System.IO.Ports;
using ReactiveUI;
using View_WPF.ViewModels;
using System.Reactive;
using System.Reactive.Linq;
using View_WPF.ViewModels.Settings;
using View_WPF.Views.ServiceWindows;

namespace View_WPF.Views.Settings
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool SettingsIsChanged { get; private set; } = false;
        public string SettingsDocument { get; private set; }

        private Page_IP? Settings_IP;
        private Page_SerialPort? Settings_SerialPort;

        internal readonly ViewModel_Settings ViewModel;


        public SettingsWindow()
        {
            InitializeComponent();

            ViewModel = new ViewModel_Settings(
                MessageView.Show,
                MessageView.ShowDialog,
                Get_FilePath,
                Get_NewFileName
                );

            DataContext = ViewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Settings_SerialPort = new Page_SerialPort(ViewModel.SerialPort_VM)
            {
                Height = Frame_Settings.ActualHeight,
                Width = Frame_Settings.ActualWidth,

                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Settings_IP = new Page_IP(ViewModel.Ethernet_VM)
            {
                Height = Frame_Settings.ActualHeight,
                Width = Frame_Settings.ActualWidth,

                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            if (ViewModel != null)
            {
                await ViewModel.Command_Loaded.Execute();
            }
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:

                    if (ViewModel != null)
                    {
                        await ViewModel.Command_File_Save.Execute();
                    }
                    
                    break;

                case Key.Escape:
                    Close();
                    break;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RadioButton_SerialPort_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_Settings.Navigate(Settings_SerialPort) == false)
            {
                MessageBox.Show("Не удалось перейти на страницу " + Settings_SerialPort?.Name, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private void RadioButton_Ethernet_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame_Settings.Navigate(Settings_IP) == false)
            {
                MessageBox.Show("Не удалось перейти на страницу " + Settings_IP?.Name, this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private string Get_NewFileName()
        {
            EnterTextWindow window = new EnterTextWindow()
            {
                Owner = this
            };

            window.ShowDialog();

            return window.FileName;
        }

        private string? Get_FilePath(string Title)
        {
            Microsoft.Win32.OpenFileDialog FileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = Title,
                Filter = "Файл настроек|*.json"
            };

            // Show open file dialog box
            Nullable<bool> result = FileDialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                return FileDialog.FileName;
            }

            return null;
        }
    }
}
