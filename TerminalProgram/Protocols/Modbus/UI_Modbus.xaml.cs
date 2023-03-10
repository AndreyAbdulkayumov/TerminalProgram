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

        private ModbusMessage ModbusMessageType = null;

        private readonly string MainWindowTitle;

        private readonly ObservableCollection<ModbusDataDisplayed> DataDisplayedList = 
            new ObservableCollection<ModbusDataDisplayed>();

        private NumberStyles Address_NumberStyle = NumberStyles.HexNumber;
        private NumberStyles Data_NumberStyle = NumberStyles.HexNumber;

        private UInt16 PackageNumber = 0;

        private byte SelectedSlaveID = 0;
        private UInt16 SelectedAddress = 0;
        private UInt16 NumberOfRegisters = 1;

        private const UInt16 CRC_Polynom = 0xA001;
        private bool CRC_Enable = false;

        private readonly List<UInt16> WriteBuffer = new List<UInt16>();
        private string WriteDataText = String.Empty;

        // Значения по умолчанию (самые частоиспользуемые функции).
        private ModbusReadFunction ReadFunction = Function.ReadInputRegisters;
        private ModbusWriteFunction WriteFunction = Function.PresetSingleRegister;


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

            foreach (ModbusReadFunction element in Function.AllReadFunctions)
            {
                ComboBox_ReadFunc.Items.Add(element.DisplayedName);
            }

            foreach (ModbusWriteFunction element in Function.AllWriteFunctions)
            {
                ComboBox_WriteFunc.Items.Add(element.DisplayedName);
            }

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

                WriteBuffer.Clear();

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

            CheckBox_CRC_Enable.Visibility = Visibility.Visible;
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

                if (NumberOfRegisters < 1)
                {
                    MessageBox.Show("Сколько, сколько регистров вы хотите прочитать? :)", MainWindowTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                MessageData DataForRead = new MessageData(
                    SelectedSlaveID,
                    SelectedAddress,
                    NumberOfRegisters,
                    ModbusMessageType is ModbusTCP_Message ? false : CRC_Enable,
                    CRC_Polynom);

                UInt16[] ModbusReadData = ModbusDevice.ReadRegister(
                                ReadFunction,
                                DataForRead,
                                ModbusMessageType,
                                out CommonResponse);

                DataDisplayedList.Add(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = ReadFunction.DisplayedNumber,
                    Address = SelectedAddress,
                    ViewAddress = CreateViewAddress(SelectedAddress, ModbusReadData.Length),
                    Data = ModbusReadData,
                    ViewData = CreateViewData(ModbusReadData)
                });

                DataGrid_ModbusData.ScrollIntoView(DataDisplayedList.Last());

                PackageNumber++;
            }

            catch(ModbusException error)
            {
                ModbusErrorHandler(error);
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

        private string CreateViewAddress(UInt16 StartAddress, int NumberOfRegisters)
        {
            string DisplayedString = String.Empty;

            UInt16 CurrentAddress = StartAddress;

            for (int i = 0; i < NumberOfRegisters; i++)
            {
                DisplayedString += "0x" + CurrentAddress.ToString("X") +
                    " (" + CurrentAddress.ToString() + ")";

                if (i != NumberOfRegisters - 1)
                {
                    DisplayedString += "\n";
                }

                CurrentAddress++;
            }

            return DisplayedString;
        }

        private string CreateViewData(UInt16[] ModbusData)
        {
            string DisplayedString = String.Empty;

            for (int i = 0; i < ModbusData.Length; i++)
            {
                DisplayedString += "0x" + ModbusData[i].ToString("X") +
                    " (" + ModbusData[i].ToString() + ")";

                if (i != ModbusData.Length - 1)
                {
                    DisplayedString += "\n";
                }
            }

            return DisplayedString;
        }

        private void ModbusErrorHandler(ModbusException error)
        {
            DataDisplayedList.Add(new ModbusDataDisplayed()
            {
                OperationID = PackageNumber,
                FuncNumber = ReadFunction.DisplayedNumber,
                Address = SelectedAddress,
                ViewAddress = CreateViewAddress(SelectedAddress, 1),
                Data = new UInt16[1],
                ViewData = "Ошибка Modbus.\nКод: " + error.ErrorCode.ToString()
            });

            DataGrid_ModbusData.ScrollIntoView(DataDisplayedList.Last());

            PackageNumber++;

            MessageBox.Show("Ошибка Modbus.\n\n" +
                "Код функции: " + error.FunctionCode.ToString() + "\n" +
                "Код ошибки: " + error.ErrorCode.ToString() + "\n\n" +
                error.Message,
                MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
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

                UInt16[] ModbusWriteData;

                if (WriteFunction == Function.PresetMultipleRegister)
                {
                    ModbusWriteData = WriteBuffer.ToArray();
                }

                else
                {
                    ModbusWriteData = new UInt16[1]
                    {
                        CheckNumber(TextBox_Data, Data_NumberStyle)
                    };
                }

                MessageData DataForWrite = new MessageData(
                    SelectedSlaveID,
                    SelectedAddress,
                    ModbusWriteData,
                    ModbusMessageType is ModbusTCP_Message ? false : CRC_Enable,
                    CRC_Polynom);

                ModbusDevice.WriteRegister(
                    WriteFunction,
                    DataForWrite,
                    ModbusMessageType,
                    out CommonResponse);

                DataDisplayedList.Add(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = WriteFunction.DisplayedNumber,
                    Address = SelectedAddress,
                    ViewAddress = CreateViewAddress(SelectedAddress, ModbusWriteData.Length),
                    Data = ModbusWriteData,
                    ViewData = CreateViewData(ModbusWriteData)
                });

                DataGrid_ModbusData.ScrollIntoView(DataDisplayedList.Last());

                PackageNumber++;
            }

            catch (ModbusException error)
            {
                ModbusErrorHandler(error);
            }

            catch (Exception error)
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
                SelectedSlaveID = (byte)CheckNumber(TextBox_SlaveID, NumberStyles.Number);
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при изменении текста в поле \"Slave ID\":\n\n" +
                    error.Message, MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }            
        }

        private void CheckBox_CRC_Enable_Click(object sender, RoutedEventArgs e)
        {
            if (CheckBox_CRC_Enable != null)
            {
                CRC_Enable = (bool)CheckBox_CRC_Enable.IsChecked;
            }
        }

        private void Button_ClearDataGrid_Click(object sender, RoutedEventArgs e)
        {
            DataDisplayedList.Clear();
        }

        private void TextBox_Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SelectedAddress = CheckNumber(TextBox_Address, Address_NumberStyle);

                TextBox_Address.Text = TextBox_Address.Text.ToUpper();
                TextBox_Address.SelectionStart = TextBox_Address.Text.Length;
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при изменении текста в поле \"Адрес\":\n\n" +
                    error.Message, MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBox_NumberOfRegisters_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumberOfRegisters = CheckNumber(TextBox_NumberOfRegisters, NumberStyles.Integer);
        }

        private void TextBox_Data_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (WriteFunction == Function.PresetMultipleRegister)
                {
                    WriteBuffer.Clear();

                    string[] SplitString = TextBox_Data.Text.Split(' ');

                    string[] Values = SplitString.Where(element => element != "").ToArray();

                    UInt16 Buffer = 0;

                    foreach (string element in Values)
                    {
                        if (UInt16.TryParse(element, Data_NumberStyle, CultureInfo.InvariantCulture, out Buffer) == false)
                        {
                            MessageBox.Show("Ввод букв и знаков не допустим.\n\nДиапазон чисел от 0 до 65 535 (0x0000 - 0xFFFF).",
                                MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Warning);

                            TextBox_Data.Text = WriteDataText;

                            return;
                        }

                        else
                        {
                            WriteBuffer.Add(Buffer);
                        }
                    }
                }

                else
                {
                    CheckNumber(TextBox_Data, Data_NumberStyle);
                }

                int CursorPosition = TextBox_Data.SelectionStart;
                TextBox_Data.Text = TextBox_Data.Text.ToUpper();
                TextBox_Data.SelectionStart = CursorPosition;

                WriteDataText = TextBox_Data.Text;
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
                    ConvertDataTextIn(NumberStyles.HexNumber);
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
                    ConvertDataTextIn(NumberStyles.Number);
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Возникла ошибка при выборе пункта \"Десятичный\":\n\n" +
                    error.Message, MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertDataTextIn(NumberStyles Style)
        {
            string[] SplitString = TextBox_Data.Text.Split(' ');

            string[] Values = SplitString.Where(element => element != "").ToArray();

            string DataString = "";

            if (Style == NumberStyles.Number)
            {
                foreach (string element in Values)
                {
                    DataString += Int32.Parse(element, NumberStyles.HexNumber).ToString() + " ";
                }
            }

            else if (Style == NumberStyles.HexNumber)
            {
                foreach(string element in Values)
                {
                    DataString += Convert.ToInt32(element).ToString("X") + " ";
                }
            }            

            TextBox_Data.Text = DataString;
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

                TextBox_Data.Text = String.Empty;
            }

            catch (Exception error)
            {
                MessageBox.Show("Не удалось выставить функцию записи.\n\n" + error.Message, MainWindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);

                ComboBox_WriteFunc.SelectedIndex = -1;
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
