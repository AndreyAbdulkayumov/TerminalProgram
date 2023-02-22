using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Globalization;

namespace TerminalProgram.Protocols.Modbus
{
    public class ModbusDataDisplayed
    {
        public UInt16 OperationID { get; set; }
        public string FuncNumber { get; set; }
        public UInt16 Address { get; set; }
        public string ViewAddress { get; set; }
        public UInt16 Data { get; set; }
        public string ViewData { get; set; }
    }

    /// <summary>
    /// Логика взаимодействия для Modbus.xaml
    /// </summary>
    public partial class UI_Modbus : Page
    {
        public event EventHandler<EventArgs> ErrorHandler;

        public ModbusResponse CommonResponse = new ModbusResponse();

        private Modbus ModbusDevice = null;

        private int SelectedSlaveID = 0;

        private TypeOfModbus ModbusType;

        private UInt16 PackageNumber = 0;

        private readonly string MainWindowTitle;

        ObservableCollection<ModbusDataDisplayed> DataDisplayedList = new ObservableCollection<ModbusDataDisplayed>();

        private NumberStyles Address_NumberStyle = NumberStyles.HexNumber;
        private NumberStyles Data_NumberStyle = NumberStyles.HexNumber;

        public UI_Modbus(MainWindow window)
        {
            InitializeComponent();

            MainWindowTitle = window.Title;

            window.DeviceIsConnect += MainWindow_DeviceIsConnect;
            window.DeviceIsDisconnected += MainWindow_DeviceIsDisconnected;

            DataGrid_ModbusData.ItemsSource = DataDisplayedList;

            RadioButton_NumFormat_Hex.IsChecked = true;

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

            RadioButton_NumFormat_Hex.IsEnabled = true;
            RadioButton_NumFormat_Dec.IsEnabled = true;

            TextBox_Address.IsEnabled = true;
            TextBox_Data.IsEnabled = true;

            Button_Write.IsEnabled = true;
            Button_Read.IsEnabled = true;

            DataDisplayedList.Clear();
            PackageNumber = 0;
        }

        private void SetUI_Disconnected()
        {
            TextBlock_ModbusMode.Text = "не определено";

            TextBox_SlaveID.IsEnabled = false;

            RadioButton_NumFormat_Hex.IsEnabled = false;
            RadioButton_NumFormat_Dec.IsEnabled = false;

            TextBox_Address.IsEnabled = false;
            TextBox_Data.IsEnabled = false;

            TextBox_Address.Text = "";
            TextBox_Data.Text = "";

            Button_Write.IsEnabled = false;
            Button_Read.IsEnabled = false;
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

                if (TextBox_Address.Text == String.Empty)
                {
                    MessageBox.Show("Укажите адрес Modbus регистра.", MainWindowTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                ModbusDevice.SlaveID = (byte)SelectedSlaveID;

                UInt16 ModbusAddress = CheckNumber(TextBox_Address, Address_NumberStyle);

                UInt16 ModbusReadData = ModbusDevice.ReadRegister(
                                ModbusAddress,
                                out CommonResponse,
                                1,
                                ModbusType,
                                ModbusType == TypeOfModbus.TCP ? false : true);

                DataDisplayedList.Add(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = "0x04 (чтение)",
                    Address = ModbusAddress,
                    ViewAddress = "0x" + ModbusAddress.ToString("X") + " (" + ModbusAddress.ToString() + ")",
                    Data = ModbusReadData,
                    ViewData = "0x" + ModbusReadData.ToString("X") + " (" + ModbusReadData.ToString() + ")"
                });

                DataGrid_ModbusData.ScrollIntoView(DataDisplayedList.Last());

                PackageNumber++;
            }
            
            catch(Exception error)
            {
                if (ErrorHandler != null)
                {
                    ErrorHandler(this, new EventArgs());

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
                    MessageBox.Show("Укажите Slave ID.", MainWindowTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                if (TextBox_Address.Text == String.Empty)
                {
                    MessageBox.Show("Укажите адрес Modbus регистра.", MainWindowTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                if (TextBox_Data.Text == String.Empty)
                {
                    MessageBox.Show("Укажите данные для записи в Modbus регистр.", MainWindowTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                ModbusDevice.SlaveID = (byte)SelectedSlaveID;

                UInt16 ModbusAddress = CheckNumber(TextBox_Address, Address_NumberStyle);
                UInt16 ModbusWriteData = CheckNumber(TextBox_Data, Data_NumberStyle);

                ModbusDevice.WriteRegister(
                    ModbusAddress,
                    ModbusWriteData,
                    out CommonResponse,
                    ModbusType,
                    false);

                DataDisplayedList.Add(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = "0x06 (запись)",
                    Address = ModbusAddress,
                    ViewAddress = "0x" + ModbusAddress.ToString("X") + " (" + ModbusAddress.ToString() + ")",
                    Data = ModbusWriteData,
                    ViewData = "0x" + ModbusWriteData.ToString("X") + " (" + ModbusWriteData.ToString() + ")"
                });

                DataGrid_ModbusData.ScrollIntoView(DataDisplayedList.Last());

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
            try
            {
                SelectedSlaveID = CheckNumber(TextBox_SlaveID, NumberStyles.Number);
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при изменении текста в поле \"Slave ID\":\n\n" +
                    error.Message, MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        private void TextBox_Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CheckNumber(TextBox_Address, Address_NumberStyle);

                TextBox_Address.Text = TextBox_Address.Text.ToUpper();
                TextBox_Address.SelectionStart = TextBox_Address.Text.Length;
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при изменении текста в поле \"Адрес\":\n\n" +
                    error.Message, MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBox_Data_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CheckNumber(TextBox_Data, Data_NumberStyle);

                TextBox_Data.Text = TextBox_Data.Text.ToUpper();
                TextBox_Data.SelectionStart = TextBox_Data.Text.Length;
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при изменении текста в поле \"Данные\":\n\n" +
                    error.Message, MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private UInt16 CheckNumber(TextBox SelectedTextBox, NumberStyles Style)
        {
            if (SelectedTextBox.Text == String.Empty)
            {
                return 0;
            }
            
            UInt16 Number;

            while (true)
            {
                if (SelectedTextBox.Text == String.Empty)
                {
                    return 0;
                }

                if (UInt16.TryParse(SelectedTextBox.Text, Style, CultureInfo.InvariantCulture, out Number) == false)
                {
                    SelectedTextBox.Text = SelectedTextBox.Text.Remove(SelectedTextBox.Text.Length - 1);
                    SelectedTextBox.SelectionStart = SelectedTextBox.Text.Length;

                    MessageBox.Show("Ввод букв и знаков не допустим.\n\nДиапазон чисел от 0 до 65 535 (0x0000 - 0xFFFF).",
                        MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                else
                {
                    break;
                }
            }

            return Number;
        }

        private void RadioButton_NumFormat_Hex_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                Address_NumberStyle = NumberStyles.HexNumber;
                Data_NumberStyle = NumberStyles.HexNumber;

                if (TextBox_Address.Text.Length > 0)
                {
                    TextBox_Address.Text = Convert.ToInt32(TextBox_Address.Text).ToString("X");
                }

                if (TextBox_Data.Text.Length > 0)
                {
                    TextBox_Data.Text = Convert.ToInt32(TextBox_Data.Text).ToString("X");
                }
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Возникла ошибка при выборе пункта \"Шестнадцатеричный\":\n\n" +
                    error.Message, MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RadioButton_NumFormat_Dec_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                Address_NumberStyle = NumberStyles.Number;
                Data_NumberStyle = NumberStyles.Number;

                if (TextBox_Address.Text.Length > 0)
                {
                    TextBox_Address.Text = Int32.Parse(TextBox_Address.Text, NumberStyles.HexNumber).ToString();
                }

                if (TextBox_Data.Text.Length > 0)
                {
                    TextBox_Data.Text = Int32.Parse(TextBox_Data.Text, NumberStyles.HexNumber).ToString();
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при выборе пункта \"Десятичный\":\n\n" +
                    error.Message, MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
