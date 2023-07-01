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
using View_WPF.ViewModels;
using View_WPF.ViewModels.MainWindow;

namespace View_WPF.Views.Protocols
{
    /// <summary>
    /// Логика взаимодействия для Modbus.xaml
    /// </summary>
    public partial class Modbus : Page
    {
        public Modbus()
        {
            InitializeComponent();

            DataContext = new ViewModel_Modbus(
                MessageView.Show,
                SetUI_Connected,
                SetUI_Disconnected,
                DataGrid_ScrollTo);
        }

        private void SetUI_Connected()
        {
            TextBox_SlaveID.IsEnabled = true;

            RadioButton_NumFormat_Hex.IsEnabled = true;
            RadioButton_NumFormat_Dec.IsEnabled = true;

            TextBox_Address.IsEnabled = true;
            TextBox_Data.IsEnabled = true;

            Button_Write.IsEnabled = true;
            Button_Read.IsEnabled = true;

            ComboBox_ReadFunc.IsEnabled = true;
            ComboBox_WriteFunc.IsEnabled = true;

            TextBox_NumberOfRegisters.IsEnabled = true;
            CheckBox_CRC_Enable.IsEnabled = true;
        }

        private void SetUI_Disconnected()
        {
            TextBox_SlaveID.IsEnabled = false;

            RadioButton_NumFormat_Hex.IsEnabled = false;
            RadioButton_NumFormat_Dec.IsEnabled = false;

            TextBox_Address.IsEnabled = false;
            TextBox_Data.IsEnabled = false;

            TextBox_Address.Text = "";
            TextBox_Data.Text = "";
            TextBox_NumberOfRegisters.Text = "";

            Button_Write.IsEnabled = false;
            Button_Read.IsEnabled = false;

            ComboBox_ReadFunc.IsEnabled = false;
            ComboBox_WriteFunc.IsEnabled = false;

            TextBox_NumberOfRegisters.IsEnabled = false;
            CheckBox_CRC_Enable.IsEnabled = false;
        }

        private void DataGrid_ScrollTo(ModbusDataDisplayed Item)
        {
            DataGrid_ModbusData.ScrollIntoView(Item);
        }
    }
}
