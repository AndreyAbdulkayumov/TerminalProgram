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

namespace TerminalProgram.Protocols.Modbus
{
    public struct ModbusData
    {
        public int Register;
        public int Data;

        public ModbusData(int Register, int Data)
        {
            this.Register = Register;
            this.Data = Data;
        }
    }

    /// <summary>
    /// Логика взаимодействия для Modbus.xaml
    /// </summary>
    public partial class UI_Modbus : Page
    {
        public event EventHandler<EventArgs> ErrorHandler;

        public DEVICE_RESPONSE CommonResponse = new DEVICE_RESPONSE();

        private List<ModbusData> DataList = new List<ModbusData>();

        private Modbus ModbusDevice = null;

        private TypeOfModbus ModbusType;

        private UInt16 PackageNumber = 0;

        private readonly string MainWindowTitle;

        public UI_Modbus(MainWindow window)
        {
            InitializeComponent();

            MainWindowTitle = window.Title;

            window.DeviceIsConnect += MainWindow_DeviceIsConnect;
            window.DeviceIsDisconnected += MainWindow_DeviceIsDisconnected;

            SetUI_Disconnected();
        }

        private void MainWindow_DeviceIsConnect(object sender, ConnectArgs e)
        {
            if (e.ConnectedDevice.IsConnected)
            {
                ModbusDevice = new Modbus(e.ConnectedDevice);

                if (e.ConnectedDevice is IPClient)
                {
                    ModbusType = TypeOfModbus.TCP;
                }

                else if (e.ConnectedDevice is SerialPortClient)
                {
                    ModbusType = TypeOfModbus.RTU;
                }

                else
                {
                    throw new Exception("Задан неизвестный тип подключения: " + e.ConnectedDevice.ToString());
                }

                SetUI_Connected();
            }            
        }

        private void MainWindow_DeviceIsDisconnected(object sender, ConnectArgs e)
        {
            SetUI_Disconnected();
        }

        private void SetUI_Connected()
        {
            TextBlock_ModbusMode.Text = "Modbus " + ModbusType.ToString();

            TextBox_SlaveID.IsEnabled = true;

            TextBox_Address.IsEnabled = true;
            TextBox_Data.IsEnabled = true;

            Button_Write.IsEnabled = true;
            Button_Read.IsEnabled = true;
        }

        private void SetUI_Disconnected()
        {
            TextBlock_ModbusMode.Text = "не определено";

            TextBox_SlaveID.IsEnabled = false;

            TextBox_Address.IsEnabled = false;
            TextBox_Data.IsEnabled = false;

            TextBox_Address.Text = "";
            TextBox_Data.Text = "";

            Button_Write.IsEnabled = false;
            Button_Read.IsEnabled = false;

            TextBlock_RX.Text = "";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataList.Add(new ModbusData(1, 0));
            DataList.Add(new ModbusData(2, 43));
            DataList.Add(new ModbusData(1, 43));
            DataList.Add(new ModbusData(4, 0));

            //DataGrid_History.ItemsSource = DataList;
        }

        private void Button_Read_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBox_SlaveID.Text == String.Empty)
                {
                    MessageBox.Show("Укажите Slave ID.", MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                TextBlock_RX.Text = "";

                UInt16 Data = ModbusDevice.ReadRegister(
                                PackageNumber,
                                Convert.ToUInt16(TextBox_Address.Text),
                                out CommonResponse,
                                2,
                                ModbusType,
                                ModbusType == TypeOfModbus.TCP ? false : true);

                PackageNumber++;

                TextBlock_RX.Text = Data.ToString();
            }
            
            catch(Exception error)
            {
                if (ErrorHandler != null)
                {
                    //ErrorHandler(this, new EventArgs());

                    MessageBox.Show("Возникла ошибка при нажатии нажатии на кнопку \"Прочитать\": \n\n" +
                        error.Message + "\n\nКлиент был отключен.", 
                        MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else
                {
                    MessageBox.Show("Возникла ошибка при нажатии нажатии на кнопку \"Прочитать\": \n\n" +
                        error.Message + "\n\nКлиент не был отключен.",
                        MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Button_Write_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBox_SlaveID.Text == String.Empty)
                {
                    MessageBox.Show("Укажите Slave ID.", MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                TextBlock_RX.Text = "";

                ModbusDevice.WriteRegister(
                    PackageNumber,
                    Convert.ToUInt16(TextBox_Address.Text),
                    Convert.ToUInt16(TextBox_Data.Text),
                    out CommonResponse,
                    2,
                    ModbusType,
                    false);

                PackageNumber++;
            }
            
            catch(Exception error)
            {
                if (ErrorHandler != null)
                {
                    ErrorHandler(this, new EventArgs());

                    MessageBox.Show("Возникла ошибка при нажатии на кнопку \"Записать\":\n\n" +
                        error.Message + "\n\nКлиент был отключен.",
                        MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }

                else
                {
                    MessageBox.Show("Возникла ошибка при нажатии на кнопку \"Записать\":\n\n" +
                        error.Message + "\n\nКлиент не был отключен.",
                        MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TextBox_SlaveID_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckNumber(TextBox_SlaveID);
        }

        private void TextBox_Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckNumber(TextBox_Address);
        }

        private void TextBox_Data_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckNumber(TextBox_Data);
        }

        private void CheckNumber(TextBox SelectedTextBox)
        {
            string TextData = SelectedTextBox.Text;

            if (TextData == "")
            {
                return;
            }

            if (UInt16.TryParse(TextData, out _) == false)
            {
                SelectedTextBox.Text = SelectedTextBox.Text.Remove(SelectedTextBox.Text.Length - 1);
                SelectedTextBox.SelectionStart = SelectedTextBox.Text.Length;
                
                MessageBox.Show("Ввод букв и знаков не допустим",
                    MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        
    }
}
