using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Core.Models;
using Core.Models.Modbus;
using Core.Models.Modbus.Message;
using System.Globalization;
using System.Reactive.Linq;
using Core.Clients;
using MessageBox_Core;

namespace ViewModels.MainWindow
{
    public class ModbusDataDisplayed
    {
        public UInt16 OperationID { get; set; }
        public string? FuncNumber { get; set; }
        public UInt16 Address { get; set; }
        public string? ViewAddress { get; set; }
        public UInt16[]? Data { get; set; }
        public string? ViewData { get; set; }
    }

    public class ViewModel_Modbus : ReactiveObject
    {
        public static event EventHandler<ModbusDataDisplayed>? AddDataInView;

        #region Properties

        private const string ModbusMode_Name_Default = "не определен";

        private string? _modbusMode_Name;

        public string? ModbusMode_Name
        {
            get => _modbusMode_Name;
            set => this.RaiseAndSetIfChanged(ref _modbusMode_Name, value);
        }

        private const string Modbus_RTU_Name = "Modbus RTU";
        private const string Modbus_ASCII_Name = "Modbus ASCII";

        private readonly ObservableCollection<string> _modbus_RTU_ASCII =
            new ObservableCollection<string>()
            {
                Modbus_RTU_Name, Modbus_ASCII_Name
            };

        public ObservableCollection<string> Modbus_RTU_ASCII
        {
            get => _modbus_RTU_ASCII;
        }

        private string? _selected_Modbus_RTU_ASCII;

        public string? Selected_Modbus_RTU_ASCII
        {
            get => _selected_Modbus_RTU_ASCII;
            set => this.RaiseAndSetIfChanged(ref _selected_Modbus_RTU_ASCII, value);
        }

        private bool _connection_IsSerialPort = false;

        public bool Connection_IsSerialPort
        {
            get => _connection_IsSerialPort;
            set => this.RaiseAndSetIfChanged(ref _connection_IsSerialPort, value);
        }

        private string? _requestBytesDisplayed;

        public string? RequestBytesDisplayed
        {
            get => _requestBytesDisplayed;
            set => this.RaiseAndSetIfChanged(ref _requestBytesDisplayed, value);
        }

        private string? _responseBytesDisplayed;

        public string? ResponseBytesDisplayed
        {
            get => _responseBytesDisplayed;
            set => this.RaiseAndSetIfChanged(ref _responseBytesDisplayed, value);
        }

        private string? _slaveID;

        public string? SlaveID
        {
            get => _slaveID;
            set => this.RaiseAndSetIfChanged(ref _slaveID, value);
        }

        private bool _checkSum_Enable;

        public bool CheckSum_Enable
        {
            get => _checkSum_Enable;
            set => this.RaiseAndSetIfChanged(ref _checkSum_Enable, value);
        }

        private bool _checkSum_IsVisible;

        public bool CheckSum_IsVisible
        {
            get => _checkSum_IsVisible;
            set => this.RaiseAndSetIfChanged(ref _checkSum_IsVisible, value);
        }

        private bool _selectedNumberFormat_Hex;

        public bool SelectedNumberFormat_Hex
        {
            get => _selectedNumberFormat_Hex;
            set => this.RaiseAndSetIfChanged(ref _selectedNumberFormat_Hex, value);
        }

        private bool _selectedNumberFormat_Dec;

        public bool SelectedNumberFormat_Dec
        {
            get => _selectedNumberFormat_Dec;
            set => this.RaiseAndSetIfChanged(ref _selectedNumberFormat_Dec, value);
        }

        private string? _numberFormat;

        public string? NumberFormat
        {
            get => _numberFormat;
            set => this.RaiseAndSetIfChanged(ref _numberFormat, value);
        }

        private string? _address;

        public string? Address
        {
            get => _address;
            set => this.RaiseAndSetIfChanged(ref _address, value);
        }

        private string? _numberOfRegisters;

        public string? NumberOfRegisters
        {
            get => _numberOfRegisters;
            set => this.RaiseAndSetIfChanged(ref _numberOfRegisters, value);
        }

        private string? _writeData;

        public string? WriteData
        {
            get => _writeData;
            set => this.RaiseAndSetIfChanged(ref _writeData, value);
        }

        private ObservableCollection<string> _readFunctions = new ObservableCollection<string>();

        public ObservableCollection<string> ReadFunctions
        {
            get => _readFunctions;
            set => this.RaiseAndSetIfChanged(ref _readFunctions, value);
        }

        private string? _selectedReadFunction;

        public string? SelectedReadFunction
        {
            get => _selectedReadFunction;
            set => this.RaiseAndSetIfChanged(ref _selectedReadFunction, value);
        }

        private ObservableCollection<string> _writeFunctions = new ObservableCollection<string>();

        public ObservableCollection<string> WriteFunctions
        {
            get => _writeFunctions;
            set => this.RaiseAndSetIfChanged(ref _writeFunctions, value);
        }

        private string? _selectedWriteFunction;

        public string? SelectedWriteFunction
        {
            get => _selectedWriteFunction;
            set => this.RaiseAndSetIfChanged(ref _selectedWriteFunction, value);
        }

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> Command_Write { get; }
        public ReactiveCommand<Unit, Unit> Command_Read { get; }
        public ReactiveCommand<Unit, Unit> Command_ClearDataGrid { get; }

        #endregion

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;

        private readonly Action SetUI_Connected;
        private readonly Action SetUI_Disconnected;

        public static ModbusMessage? ModbusMessageType { get; private set; }

        private readonly List<UInt16> WriteBuffer = new List<UInt16>();

        private NumberStyles NumberViewStyle;

        public static UInt16 PackageNumber { get; private set; } = 0;

        private byte SelectedSlaveID = 0;
        private UInt16 SelectedAddress = 0;
        private UInt16 SelectedNumberOfRegisters = 1;

        public const UInt16 CRC16_Polynom = 0xA001;

        private ModbusFunction? CurrentFunction;


        public ViewModel_Modbus(
            Action<string, MessageType> MessageBox,
            Action ClearDataGrid_Handler,
            Action UI_Connected_Handler,
            Action UI_Disconnected_Handler
            )
        {
            Message = MessageBox;

            SetUI_Connected = UI_Connected_Handler;
            SetUI_Disconnected = UI_Disconnected_Handler;

            Model = ConnectedHost.Model;

            SetUI_Disconnected.Invoke();

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;


            /****************************************************/
            //
            // Первоначальная настройка UI
            //
            /****************************************************/

            Selected_Modbus_RTU_ASCII = Modbus_RTU_ASCII.First();

            ModbusMode_Name = ModbusMode_Name_Default;

            CheckSum_Enable = true;
            CheckSum_IsVisible = true;

            SelectedNumberFormat_Hex = true;

            foreach (ModbusReadFunction element in Function.AllReadFunctions)
            {
                ReadFunctions.Add(element.DisplayedName);
            }

            SelectedReadFunction = Function.ReadInputRegisters.DisplayedName;

            foreach (ModbusWriteFunction element in Function.AllWriteFunctions)
            {
                WriteFunctions.Add(element.DisplayedName);
            }

            SelectedWriteFunction = Function.PresetSingleRegister.DisplayedName;


            /****************************************************/
            //
            // Настройка свойств и команд модели отображения
            //
            /****************************************************/


            Command_ClearDataGrid = ReactiveCommand.Create(ClearDataGrid_Handler.Invoke);
            Command_ClearDataGrid.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка очистки содержимого таблицы.\n\n" + error.Message, MessageType.Error));

            Command_Write = ReactiveCommand.Create(Modbus_Write);
            Command_Read = ReactiveCommand.Create(Modbus_Read);

            this.WhenAnyValue(x => x.Selected_Modbus_RTU_ASCII)
                .WhereNotNull()
                .Subscribe(x =>
                {
                    if (Model.HostIsConnect)
                    {
                        if (Selected_Modbus_RTU_ASCII == Modbus_RTU_Name)
                        {
                            ModbusMessageType = new ModbusRTU_Message();
                        }

                        else if (Selected_Modbus_RTU_ASCII == Modbus_ASCII_Name)
                        {
                            ModbusMessageType = new ModbusASCII_Message();
                        }

                        else
                        {
                            Message.Invoke("Задан неизвестный тип Modbus протокола: " + Selected_Modbus_RTU_ASCII, MessageType.Error);
                            return;
                        }
                    }
                });

            this.WhenAnyValue(x => x.SlaveID)
                .WhereNotNull()
                .Select(x => StringValue.CheckNumber(x, NumberStyles.Number, out SelectedSlaveID))
                .Subscribe(x => SlaveID = x);

            this.WhenAnyValue(x => x.SelectedNumberFormat_Hex, x => x.SelectedNumberFormat_Dec)
                .Subscribe(values =>
                {
                    if (values.Item1 == true && values.Item2 == true)
                    {
                        return;
                    }

                    // Выбран шестнадцатеричный формат числа в полях Адрес и Данные
                    if (values.Item1)
                    {
                        SelectNumberFormat_Hex();
                    }

                    // Выбран десятичный формат числа в полях Адрес и Данные
                    else if (values.Item2)
                    {
                        SelectNumberFormat_Dec();
                    }
                });

            this.WhenAnyValue(x => x.Address)
                .WhereNotNull()
                .Select(x => StringValue.CheckNumber(x, NumberViewStyle, out SelectedAddress))
                .Subscribe(x => Address = x.ToUpper());

            this.WhenAnyValue(x => x.NumberOfRegisters)
                .WhereNotNull()
                .Select(x => StringValue.CheckNumber(x, NumberStyles.Number, out SelectedNumberOfRegisters))
                .Subscribe(x => NumberOfRegisters = x);

            this.WhenAnyValue(x => x.SelectedWriteFunction)
                .WhereNotNull()
                .Where(x => x != String.Empty)
                .Subscribe(_ => WriteData = String.Empty);

            this.WhenAnyValue(x => x.WriteData)
                .WhereNotNull()
                .Subscribe(x => WriteData = WriteData_TextChanged(x));
        }

        public void SelectNumberFormat_Hex()
        {
            NumberFormat = "(hex)";
            NumberViewStyle = NumberStyles.HexNumber;

            if (Address != null)
            {
                Address = Convert.ToInt32(Address).ToString("X");
            }            

            WriteData = ConvertDataTextIn(NumberViewStyle, WriteData);
        }

        private void SelectNumberFormat_Dec()
        {
            NumberFormat = "(dec)";
            NumberViewStyle = NumberStyles.Number;

            if (Address != null)
            {
                Address = Int32.Parse(Address, NumberStyles.HexNumber).ToString();
            }            

            WriteData = ConvertDataTextIn(NumberViewStyle, WriteData);
        }

        private string ConvertDataTextIn(NumberStyles Style, string? Text)
        {
            if (Text == null)
            {
                return String.Empty;
            }

            string[] SplitString = Text.Split(' ');

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
                foreach (string element in Values)
                {
                    DataString += Convert.ToInt32(element).ToString("X") + " ";
                }
            }

            return DataString;
        }

        private string WriteData_TextChanged(string EnteredText)
        {
            try
            {
                if (SelectedWriteFunction == Function.PresetMultipleRegister.DisplayedName)
                {
                    WriteBuffer.Clear();

                    string[] SplitString = EnteredText.Split(' ');

                    string[] Values = SplitString.Where(element => element != "").ToArray();

                    UInt16 Buffer = 0;

                    for (int i = 0; i < Values.Length; i++)
                    {
                        Values[i] = StringValue.CheckNumber(Values[i], NumberViewStyle, out Buffer);
                        WriteBuffer.Add(Buffer);
                    }

                    // Если при второй итерации последний элемент в SplitString равен "",
                    // то в конце был пробел.
                    return string.Join(" ", Values).ToUpper() + (SplitString.Last() == "" ? " " : "");
                }

                else
                {
                    return StringValue.CheckNumber(WriteData, NumberViewStyle, out UInt16 _).ToUpper();
                }
            }

            catch (Exception error)
            {
                Message.Invoke("Возникла ошибка при изменении текста в поле \"Данные\":\n\n" +
                    error.Message, MessageType.Error);

                return String.Empty;
            }
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice is IPClient)
            {
                ModbusMessageType = new ModbusTCP_Message();

                CheckSum_IsVisible = false;

                Connection_IsSerialPort = false;
            }

            else if (e.ConnectedDevice is SerialPortClient)
            {
                if (Selected_Modbus_RTU_ASCII == Modbus_RTU_Name)
                {
                    ModbusMessageType = new ModbusRTU_Message();
                }
                
                else if (Selected_Modbus_RTU_ASCII == Modbus_ASCII_Name)
                {
                    ModbusMessageType = new ModbusASCII_Message();
                }

                else
                {
                    Message.Invoke("Задан неизвестный тип Modbus протокола: " + Selected_Modbus_RTU_ASCII, MessageType.Error);
                    return;
                }

                CheckSum_IsVisible = true;

                Connection_IsSerialPort = true;
            }

            else
            {
                Message.Invoke("Задан неизвестный тип подключения.", MessageType.Error);
                return;
            }

            WriteBuffer.Clear();

            RequestBytesDisplayed = String.Empty;
            ResponseBytesDisplayed = String.Empty;

            ModbusMode_Name = ModbusMessageType.ProtocolName;

            SetUI_Connected.Invoke();
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            SetUI_Disconnected.Invoke();

            CheckSum_IsVisible = true;

            Connection_IsSerialPort = false;

            ModbusMode_Name = ModbusMode_Name_Default;

            PackageNumber = 0;
        }

        private void Modbus_Write()
        {
            byte[] RequestBytes = new byte[0];
            byte[] ResponseBytes = new byte[0];

            try
            {
                if (Model.Modbus == null)
                {
                    Message.Invoke("Не инициализирован Modbus клиент.", MessageType.Warning);
                    return;
                }

                if (ModbusMessageType == null)
                {
                    Message.Invoke("Не задан тип протокола Modbus.", MessageType.Warning);
                    return;
                }

                if (SlaveID == null || SlaveID == String.Empty)
                {
                    Message.Invoke("Укажите Slave ID.", MessageType.Warning);
                    return;
                }

                if (Address == null || Address == String.Empty)
                {
                    Message.Invoke("Укажите адрес Modbus регистра.", MessageType.Warning);
                    return;
                }

                if (WriteData == null || WriteData == String.Empty)
                {
                    Message.Invoke("Укажите данные для записи в Modbus регистр.", MessageType.Warning);
                    return;
                }

                ModbusWriteFunction WriteFunction = Function.AllWriteFunctions.Single(x => x.DisplayedName == SelectedWriteFunction);

                CurrentFunction = WriteFunction;

                UInt16[] ModbusWriteData;

                if (WriteFunction == Function.PresetMultipleRegister)
                {
                    ModbusWriteData = WriteBuffer.ToArray();
                }

                else
                {
                    ModbusWriteData = new UInt16[1];

                    StringValue.CheckNumber(WriteData, NumberViewStyle, out ModbusWriteData[0]);
                }
                                
                MessageData Data = new WriteTypeMessage(
                    SelectedSlaveID,
                    SelectedAddress,
                    ModbusWriteData,
                    ModbusMessageType is ModbusTCP_Message ? false : CheckSum_Enable,
                    CRC16_Polynom);                                

                Model.Modbus.WriteRegister(
                    WriteFunction, 
                    Data,
                    ModbusMessageType,
                    out RequestBytes,
                    out ResponseBytes);

                ViewRequestAndResponse(RequestBytes, ResponseBytes);

                AddResponseInDataGrid(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = WriteFunction.DisplayedNumber,
                    Address = SelectedAddress,
                    ViewAddress = CreateViewAddress(SelectedAddress, ModbusWriteData.Length),
                    Data = ModbusWriteData,
                    ViewData = CreateViewData(ModbusWriteData)
                });
            }

            catch (ModbusException error)
            {
                ViewRequestAndResponse(RequestBytes, ResponseBytes);

                ModbusErrorHandler(error);
            }

            catch (Exception error)
            {
                if (RequestBytes.Length > 0 || ResponseBytes.Length > 0)
                {
                    ViewRequestAndResponse(RequestBytes, ResponseBytes);
                }

                Message.Invoke("Возникла ошибка при нажатии на кнопку \"Записать\":\n\n" + error.Message, MessageType.Error);
            }
        }

        private void Modbus_Read()
        {
            byte[] RequestBytes = Array.Empty<byte>();
            byte[] ResponseBytes = Array.Empty<byte>();

            try
            {
                if (Model.Modbus == null)
                {
                    Message.Invoke("Не инициализирован Modbus клиент.", MessageType.Warning);
                    return;
                }

                if (ModbusMessageType == null)
                {
                    Message.Invoke("Не задан тип протокола Modbus.", MessageType.Warning);
                    return;
                }

                if (SlaveID == null || SlaveID == String.Empty)
                {
                    Message.Invoke("Укажите Slave ID.", MessageType.Warning);
                    return;
                }

                if (Address == null || Address == String.Empty)
                {
                    Message.Invoke("Укажите адрес Modbus регистра.", MessageType.Warning);
                    return;
                }

                if (NumberOfRegisters == null || NumberOfRegisters == String.Empty)
                {
                    Message.Invoke("Укажите количество регистров для чтения.", MessageType.Warning);
                    return;
                }

                if (SelectedNumberOfRegisters < 1)
                {
                    Message.Invoke("Сколько, сколько регистров вы хотите прочитать? :)", MessageType.Warning);
                    return;
                }

                ModbusReadFunction ReadFunction = Function.AllReadFunctions.Single(x => x.DisplayedName == SelectedReadFunction);

                CurrentFunction = ReadFunction;               

                MessageData Data = new ReadTypeMessage(
                    SelectedSlaveID,
                    SelectedAddress,
                    SelectedNumberOfRegisters,
                    ModbusMessageType is ModbusTCP_Message ? false : CheckSum_Enable,
                    CRC16_Polynom);


                UInt16[] ModbusReadData = Model.Modbus.ReadRegister(
                                ReadFunction,
                                Data,
                                ModbusMessageType,
                                out RequestBytes,
                                out ResponseBytes);

                ViewRequestAndResponse(RequestBytes, ResponseBytes);

                AddResponseInDataGrid(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = ReadFunction.DisplayedNumber,
                    Address = SelectedAddress,
                    ViewAddress = CreateViewAddress(SelectedAddress, ModbusReadData.Length),
                    Data = ModbusReadData,
                    ViewData = CreateViewData(ModbusReadData)
                });
            }

            catch (ModbusException error)
            {
                ViewRequestAndResponse(RequestBytes, ResponseBytes);

                ModbusErrorHandler(error);
            }

            catch (Exception error)
            {
                if (RequestBytes.Length > 0 || ResponseBytes.Length > 0)
                {
                    ViewRequestAndResponse(RequestBytes, ResponseBytes);
                }

                Message.Invoke("Возникла ошибка при нажатии нажатии на кнопку \"Прочитать\": \n\n" + error.Message, MessageType.Error);
            }
        }

        private void ViewRequestAndResponse(byte[] RequestBytes, byte[] ResponseBytes)
        {
            string Request = String.Empty;

            foreach (var element in RequestBytes)
            {
                Request += element.ToString("X2") + " ";
            }

            RequestBytesDisplayed = Request;

            string Response = String.Empty;

            foreach (var element in ResponseBytes)
            {
                Response += element.ToString("X2") + " ";
            }

            ResponseBytesDisplayed = Response;
        }

        private void ModbusErrorHandler(ModbusException error)
        {
            AddResponseInDataGrid(new ModbusDataDisplayed()
            {
                OperationID = PackageNumber,
                FuncNumber = CurrentFunction?.DisplayedNumber,
                Address = SelectedAddress,
                ViewAddress = CreateViewAddress(SelectedAddress, 1),
                Data = new UInt16[1],
                ViewData = "Ошибка Modbus.\nКод: " + error.ErrorCode.ToString()
            });

            Message.Invoke(
                "Ошибка Modbus.\n\n" +
                "Код функции: " + error.FunctionCode.ToString() + "\n" +
                "Код ошибки: " + error.ErrorCode.ToString() + "\n\n" +
                error.Message,
                MessageType.Error);
        }

        /*************************************************************************/
        //
        // Следующие три метода используются в ViewModel_Modbus_CycleMode
        //
        /*************************************************************************/

        public static void AddResponseInDataGrid(ModbusDataDisplayed Data)
        {
            AddDataInView?.Invoke(null, Data);

            PackageNumber++;
        }

        public static string CreateViewAddress(UInt16 StartAddress, int NumberOfRegisters)
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

        public static string CreateViewData(UInt16[] ModbusData)
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
    }
}
