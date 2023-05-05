using Core.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace View_WPF.Protocols
{
    /// <summary>
    /// Логика взаимодействия для Modbus.xaml
    /// </summary>
    public partial class Modbus : Page
    {
        private readonly string MainWindowTitle;

        public Modbus(MainWindow window)
        {
            InitializeComponent();

            MainWindowTitle = window.Title;

            DataContext = new ViewModel_Modbus(MessageBoxView);
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

            MessageBox.Show(Message, MainWindowTitle, MessageBoxButton.OK, Image);
        }

        private void TextBox_SlaveID_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBox_CRC_Enable_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_ClearDataGrid_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_NumFormat_Hex_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_NumFormat_Dec_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_Address_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_NumberOfRegisters_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_Data_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ComboBox_ReadFunc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_WriteFunc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
