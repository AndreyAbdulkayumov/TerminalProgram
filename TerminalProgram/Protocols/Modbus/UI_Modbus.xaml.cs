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
using TerminalProgram.Protocols.Modbus.Message;

namespace TerminalProgram.Protocols.Modbus
{
    public class ModbusDataDisplayed
    {
        public UInt16 OperationID { get; set; }
        public string FuncNumber { get; set; }
        public UInt16 Address { get; set; }
        public string ViewAddress { get; set; }
        public UInt16[] Data { get; set; }
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

        private ModbusMessage ModbusMessageType;

        private UInt16 PackageNumber = 0;

        private readonly string MainWindowTitle;

        private readonly ObservableCollection<ModbusDataDisplayed> DataDisplayedList = 
            new ObservableCollection<ModbusDataDisplayed>();

        private NumberStyles Address_NumberStyle = NumberStyles.HexNumber;
        private NumberStyles Data_NumberStyle = NumberStyles.HexNumber;

        private UInt16 NumberOfRegisters = 1;

        private const UInt16 CRC_Polynom = 0xA001;
        private bool CRC_Enable = false;

        // Значения по умолчанию (самые частоиспользуемые функции).
        private ModbusReadFunction ReadFunction = Function.ReadInputRegisters;
        private ModbusWriteFunction WriteFunction = Function.PresetSingleRegister;

        private readonly string[] ReadFunctions = { 
            Function.ReadHoldingRegisters.DisplayedName,
            Function.ReadInputRegisters.DisplayedName
        };

        private readonly string[] WriteFunctions = {
            Function.PresetSingleRegister.DisplayedName,
            Function.PresetMultipleRegister.DisplayedName
        };


        public UI_Modbus(MainWindow window)
        {
            InitializeComponent();

            MainWindowTitle = window.Title;

            window.DeviceIsConnect += MainWindow_DeviceIsConnect;
            window.DeviceIsDisconnected += MainWindow_DeviceIsDisconnected;

            DataGrid_ModbusData.ItemsSource = DataDisplayedList;

            RadioButton_NumFormat_Hex.IsChecked = true;

            TextBox_NumberOfRegisters.Text = NumberOfRegisters.ToString();

            CheckBox_CRC_Enable.IsChecked = true;
            CheckBox_CRC_Enable_Click(CheckBox_CRC_Enable, new RoutedEventArgs());

            ComboBox_ReadFunc.ItemsSource = ReadFunctions;
            ComboBox_WriteFunc.ItemsSource = WriteFunctions;
                        
            ComboBox_ReadFunc.SelectedValue = ReadFunction.DisplayedName;
            ComboBox_WriteFunc.SelectedValue = WriteFunction.DisplayedName;

            SetUI_Disconnected();
        }

        private void MainWindow_DeviceIsConnect(object sender, ConnectArgs e)
        {
            if (e.ConnectedDevice.IsConnected)
            {
                ModbusDevice = new Modbus(e.ConnectedDevice);

                if (e.ConnectedDevice is IPClient)
                {
                    ModbusMessageType = new ModbusTCP_Message();

                    CheckBox_CRC_Enable.Visibility = Visibility.Hidden;
                }

                else if (e.ConnectedDevice is SerialPortClient)
                {
                    ModbusMessageType = new ModbusRTU_Message();

                    CheckBox_CRC_Enable.Visibility = Visibility.Visible;
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
            TextBlock_ModbusMode.Text = ModbusMessageType.ProtocolName;

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

            ComboBox_ReadFunc.IsEnabled = false;
            ComboBox_WriteFunc.IsEnabled = false;

            TextBox_NumberOfRegisters.IsEnabled = false;
            CheckBox_CRC_Enable.IsEnabled = false;
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

                if (TextBox_NumberOfRegisters.Text == String.Empty)
                {
                    MessageBox.Show("Введите количество регистров для чтения.", MainWindowTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                UInt16 ModbusAddress = CheckNumber(TextBox_Address, Address_NumberStyle);

                MessageData DataForRead = new MessageData(
                    (byte)SelectedSlaveID,
                    ModbusAddress,
                    NumberOfRegisters,
                    ModbusMessageType is ModbusTCP_Message ? false : CRC_Enable,
                    CRC_Polynom);

                UInt16[] ModbusReadData = ModbusDevice.ReadRegister(
                                ReadFunction,
                                DataForRead,
                                ModbusMessageType,
                                out CommonResponse);

                string ViewData = "";

                for (int i = 0; i < ModbusReadData.Length; i++)
                {
                    ViewData += "0x" + ModbusReadData[i].ToString("X") +
                        " (" + ModbusReadData[i].ToString() + ")";

                    if (i != ModbusReadData.Length - 1)
                    {
                        ViewData += ", ";
                    }
                }

                DataDisplayedList.Add(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = ReadFunction.DisplayedNumber,
                    Address = ModbusAddress,
                    ViewAddress = "0x" + ModbusAddress.ToString("X") + " (" + ModbusAddress.ToString() + ")",
                    Data = ModbusReadData,
                    ViewData = ViewData
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

                UInt16 ModbusAddress = CheckNumber(TextBox_Address, Address_NumberStyle);

                UInt16[] ModbusWriteData = new UInt16[1];
                ModbusWriteData[0] = CheckNumber(TextBox_Data, Data_NumberStyle);

                MessageData DataForWrite = new MessageData(
                    (byte)SelectedSlaveID,
                    ModbusAddress,
                    ModbusWriteData,
                    ModbusMessageType is ModbusTCP_Message ? false : CRC_Enable,
                    CRC_Polynom);

                ModbusDevice.WriteRegister(
                    WriteFunction,
                    DataForWrite,
                    ModbusMessageType,
                    out CommonResponse);

                string ViewData = "";

                for (int i = 0; i < ModbusWriteData.Length; i++)
                {
                    ViewData += "0x" + ModbusWriteData[i].ToString("X") + 
                        " (" + ModbusWriteData[i].ToString() + ")";

                    if (i != ModbusWriteData.Length - 1)
                    {
                        ViewData += ", ";
                    }
                }

                DataDisplayedList.Add(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = WriteFunction.DisplayedNumber,
                    Address = ModbusAddress,
                    ViewAddress = "0x" + ModbusAddress.ToString("X") + " (" + ModbusAddress.ToString() + ")",
                    Data = ModbusWriteData,
                    ViewData = ViewData
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

        private void ComboBox_ReadFunc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ComboBox_ReadFunc.SelectedIndex < 0)
                {
                    return;
                }

                ReadFunction = Function.AllReadFunctions.Single(
                    element => element.DisplayedName == ComboBox_ReadFunc.SelectedItem.ToString());
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Не удалось выставить функцию чтения.\n\n" + error.Message, MainWindowTitle, 
                    MessageBoxButton.OK, MessageBoxImage.Error);

                ComboBox_ReadFunc.SelectedIndex = -1;
            }
        }

        private void ComboBox_WriteFunc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ComboBox_WriteFunc.SelectedIndex < 0)
                {
                    return;
                }

                WriteFunction = Function.AllWriteFunctions.Single(
                    element => element.DisplayedName == ComboBox_WriteFunc.SelectedItem.ToString());
            }

            catch (Exception error)
            {
                MessageBox.Show("Не удалось выставить функцию записи.\n\n" + error.Message, MainWindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);

                ComboBox_WriteFunc.SelectedIndex = -1;
            }
        }

        private void TextBox_NumberOfRegisters_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumberOfRegisters = CheckNumber(TextBox_NumberOfRegisters, NumberStyles.Integer);
        }

        private void CheckBox_CRC_Enable_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_CRC_Enable != null)
            {
                CRC_Enable = (bool)CheckBox_CRC_Enable.IsChecked;
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
    }
}
