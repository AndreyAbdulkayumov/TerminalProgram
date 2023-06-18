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

            ViewModel = new ViewModel_Settings(MessageBoxView, ToolBar_File_AddExisting);

            DataContext = ViewModel;
        }


        private void MessageBoxView(string Message, MessageType Type)
        {
            MessageBoxImage Image;

            switch (Type)
            {
                case MessageType.Error:
                    Image = MessageBoxImage.Error;
                    break;

                case MessageType.Warning:
                    Image = MessageBoxImage.Warning;
                    break;

                case MessageType.Information:
                    Image = MessageBoxImage.Information;
                    break;

                default:
                    Image = MessageBoxImage.Information;
                    break;
            }

            MessageBox.Show(Message, this.Title, MessageBoxButton.OK, Image);
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

            if (await DisplaySettingsFile() == false)
            {
                Close();
            }
        }

        /// <summary>
        /// Метод отображения содержимого файла настроек на UI.
        /// </summary>
        /// <returns>
        /// true - если отображение прошло успешно, false - если пользователь проигнорировал все предупреждения.
        /// </returns>
        private async Task<bool> DisplaySettingsFile()
        {
            try
            {
                
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка чтения данных из документа. " +
                    "Проверьте его целостность или выберите другой файл настроек.\n\n" +
                    error.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                return false;
            }

            return true;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    //Button_File_Save_Click(Button_File_Save, new RoutedEventArgs());
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

        private void TextBox_Timeout_Write_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckNumber(TextBox Box)
        {
            string Text = Box.Text;

            if (Text == "")
            {
                return;
            }

            if (UInt32.TryParse(Text, out _) == false)
            {
                Box.Text = Text.Substring(0, Text.Length - 1);
                Box.SelectionStart = Box.Text.Length;

                MessageBox.Show("Разрешается вводить только неотрицательные целые числа.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void TextBox_Timeout_Read_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ComboBox_SelectedEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
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

        private void File_Save()
        {

        }

        private void File_Delete()
        {

        }

        private void ToolBar_File_AddExisting()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog FileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Добавление уже существующего файла настроек подключения",
                    Filter = "Файл настроек|*.xml" // Filter files by extension
                };

                // Show open file dialog box
                Nullable<bool> result = FileDialog.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {


                    string FileName = System.IO.Path.GetFileNameWithoutExtension(FileDialog.SafeFileName);

                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка при добавлении уже существующего файла.\n\n" + error.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void File_AddNew()
        {

        }
    }
}
