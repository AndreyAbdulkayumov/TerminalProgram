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

        private readonly string[] ArrayTypeOfEncoding = { "ASCII", "UTF-8", "UTF-32", "Unicode" };


        public SettingsWindow()
        {
            InitializeComponent();

            ComboBox_SelectedDevice.SelectionChanged -= ComboBox_SelectedDevice_SelectionChanged;


            ComboBox_SelectedDevice.SelectionChanged += ComboBox_SelectedDevice_SelectionChanged;
        }

        private void ComboBoxFilling(ComboBox Box, ref string[] Items)
        {
            Box.Items.Clear();

            foreach(string Element in Items)
            {
                Box.Items.Add(Element);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Settings_SerialPort = new Page_SerialPort()
            {
                Height = Frame_Settings.ActualHeight,
                Width = Frame_Settings.ActualWidth,

                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Settings_IP = new Page_IP()
            {
                Height = Frame_Settings.ActualHeight,
                Width = Frame_Settings.ActualWidth,

                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

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


        private void SetValue(TextBox Box, string? Value)
        {
            if (Value == null)
            {
                Box.Text = String.Empty;
                return;
            }

            Box.Text = Value;
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

        private async void ComboBox_SelectedDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
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

        private void Button_File_Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_File_Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_File_AddExisting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_File_AddNew_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
