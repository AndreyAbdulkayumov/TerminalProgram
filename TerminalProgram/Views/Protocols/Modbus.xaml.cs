using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using TerminalProgram.ViewModels;
using TerminalProgram.ViewModels.MainWindow;

namespace TerminalProgram.Views.Protocols
{
    /// <summary>
    /// Логика взаимодействия для Modbus.xaml
    /// </summary>
    public partial class Modbus : Page
    {
        private bool UI_State_IsConnected = false;

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

            Button_CycleMode.IsEnabled = true;

            RadioButton_NumFormat_Hex.IsEnabled = true;
            RadioButton_NumFormat_Dec.IsEnabled = true;

            TextBox_Address.IsEnabled = true;
            TextBox_Data.IsEnabled = true;

            Button_Write.IsEnabled = true;
            Button_Read.IsEnabled = true;

            ComboBox_ReadFunc.IsEnabled = true;
            ComboBox_WriteFunc.IsEnabled = true;

            TextBox_NumberOfRegisters.IsEnabled = true;
            CheckBox_CheckSum_Enable.IsEnabled = true;

            UI_State_IsConnected = true;
        }

        private void SetUI_Disconnected()
        {
            TextBox_SlaveID.IsEnabled = false;

            Button_CycleMode.IsEnabled = false;

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
            CheckBox_CheckSum_Enable.IsEnabled = false;

            UI_State_IsConnected = false;
        }

        private void DataGrid_ScrollTo(ModbusDataDisplayed Item)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DataGrid_ModbusData.ScrollIntoView(Item);
            }));            
        }

        private void SendUI_Enable()
        {
            if (UI_State_IsConnected == false)
            {
                return;
            }

            Button_CycleMode.IsEnabled = true;

            DockPanel_Controls.IsEnabled = true;
        }

        private void SendUI_Disable()
        {
            if (UI_State_IsConnected == false)
            {
                return;
            }

            Button_CycleMode.IsEnabled = false;

            DockPanel_Controls.IsEnabled = false;

            Button_ClearDataGrid.IsEnabled = true; 
        }

        private void Button_CycleMode_Click(object sender, RoutedEventArgs e)
        {
            Modbus_CycleMode window = new Modbus_CycleMode(SendUI_Enable);

            Application.Current.MainWindow.Closing += (sender, e) => window.Close();

            window.Show();

            SendUI_Disable();
        }
    }
}
