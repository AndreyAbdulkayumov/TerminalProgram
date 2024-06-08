using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Core.Models;
using Core.Models.Modbus;
using Core.Models.Modbus.Message;
using System.Globalization;
using System.Reactive.Linq;
using Core.Clients;
using MessageBox_Core;
using DynamicData;

namespace ViewModels.MainWindow
{
    public class RequestResponseField_ItemData
    {
        public string? ItemNumber { get; set; }
        public string? RequestDataType { get; set; }
        public string? RequestData { get; set; }
        public string? ResponseDataType { get; set; }
        public string? ResponseData { get; set; }
    }

    public class ModbusDataDisplayed
    {
        public UInt16 OperationID { get; set; }
        public string? FuncNumber { get; set; }
        public UInt16 Address { get; set; }
        public string? ViewAddress { get; set; }
        public UInt16[]? Data { get; set; }
        public string? ViewData { get; set; }
    }

    public class ViewModel_ModbusClient : ReactiveObject
    {
        private object? _currentModeViewModel;

        public object? CurrentModeViewModel
        {
            get => _currentModeViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentModeViewModel, value);
        }

        #region Properties

        private bool ui_IsEnable = false;

        public bool UI_IsEnable
        {
            get => ui_IsEnable;
            set => this.RaiseAndSetIfChanged(ref ui_IsEnable, value);
        }

        private bool _isCycleMode = false;

        public bool IsCycleMode
        {
            get => _isCycleMode;
            set => this.RaiseAndSetIfChanged(ref _isCycleMode, value);
        }

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

        private ObservableCollection<ModbusDataDisplayed> _dataInDataGrid = new ObservableCollection<ModbusDataDisplayed>();

        public ObservableCollection<ModbusDataDisplayed> DataInDataGrid
        {
            get => _dataInDataGrid;
            set => this.RaiseAndSetIfChanged(ref _dataInDataGrid, value);
        }

        private ObservableCollection<RequestResponseField_ItemData> _requestResponseItems = new ObservableCollection<RequestResponseField_ItemData>();

        public ObservableCollection<RequestResponseField_ItemData> RequestResponseItems
        {
            get => _requestResponseItems;
            set => this.RaiseAndSetIfChanged(ref _requestResponseItems, value);
        }

        private string _logData = string.Empty;
        
        public string LogData
        {
            get => _logData;
            set => this.RaiseAndSetIfChanged(ref _logData, value);
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

        public ReactiveCommand<Unit, Unit> Command_Open_ModbusScanner { get; }

        public ReactiveCommand<Unit, Unit> Command_ClearData { get; }

        public ReactiveCommand<Unit, Unit> Command_Write { get; }
        public ReactiveCommand<Unit, Unit> Command_Read { get; }        

        public ReactiveCommand<Unit, Unit> Command_Copy_Request { get; }
        public ReactiveCommand<Unit, Unit> Command_Copy_Response { get; }

        #endregion

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;

        public static ModbusMessage? ModbusMessageType { get; private set; }

        private readonly List<UInt16> WriteBuffer = new List<UInt16>();

        private NumberStyles NumberViewStyle;

        public static UInt16 PackageNumber { get; private set; } = 0;

        private byte SelectedSlaveID = 0;
        private UInt16 SelectedAddress = 0;
        private UInt16 SelectedNumberOfRegisters = 1;

        private ModbusFunction? CurrentFunction;

        private readonly ViewModel_ModbusClient_Mode_Normal Mode_Normal_VM;
        private readonly ViewModel_ModbusClient_Mode_Cycle Mode_Cycle_VM;


        public ViewModel_ModbusClient(
            Func<Task> Open_ModbusScanner,
            Action<string, MessageType> MessageBox,
            Func<string, Task> CopyToClipboard
            )
        {
            Message = MessageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Mode_Normal_VM = new ViewModel_ModbusClient_Mode_Normal(MessageBox);
            Mode_Cycle_VM = new ViewModel_ModbusClient_Mode_Cycle(MessageBox);

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

            Command_Copy_Request = ReactiveCommand.CreateFromTask(async () =>
            {
                string Data = string.Empty;

                foreach (var element in RequestResponseItems)
                {
                    if (element.RequestData != null)
                    {
                        Data += element.RequestData + " ";
                    }
                }

                await CopyToClipboard(Data);
            });
            Command_Copy_Request.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка копирования запроса в буфер обмена.\n\n" + error.Message, MessageType.Error));

            Command_Copy_Response = ReactiveCommand.CreateFromTask(async () =>
            {
                string Data = string.Empty;

                foreach (var element in RequestResponseItems)
                {
                    if (element.ResponseData != null)
                    {
                        Data += element.ResponseData + " ";
                    }
                }

                await CopyToClipboard(Data);
            });
            Command_Copy_Response.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка копирования ответа в буфер обмена.\n\n" + error.Message, MessageType.Error));

            Command_ClearData = ReactiveCommand.Create(() =>
            {
                DataInDataGrid.Clear();
                RequestResponseItems.Clear();
                LogData = string.Empty;
            });
            Command_ClearData.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка очистки данных.\n\n" + error.Message, MessageType.Error));

            Command_Write = ReactiveCommand.CreateFromTask(Modbus_Write);
            Command_Read = ReactiveCommand.CreateFromTask(Modbus_Read);

            Command_Open_ModbusScanner = ReactiveCommand.CreateFromTask(Open_ModbusScanner);

            this.WhenAnyValue(x => x.IsCycleMode)
                .Subscribe(_ =>
                {
                    CurrentModeViewModel = IsCycleMode ? Mode_Cycle_VM : Mode_Normal_VM;
                });

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
                if (SelectedWriteFunction == Function.PresetMultipleRegisters.DisplayedName ||
                    SelectedWriteFunction == Function.ForceMultipleCoils.DisplayedName)
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

            ModbusMode_Name = ModbusMessageType.ProtocolName;

            UI_IsEnable = true;
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            UI_IsEnable = false;

            CheckSum_IsVisible = true;

            Connection_IsSerialPort = false;

            ModbusMode_Name = ModbusMode_Name_Default;

            PackageNumber = 0;
        }

        private async Task Modbus_Write()
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

                if (WriteData == null || WriteData == String.Empty)
                {
                    Message.Invoke("Укажите данные для записи в Modbus регистр.", MessageType.Warning);
                    return;
                }

                ModbusWriteFunction WriteFunction = Function.AllWriteFunctions.Single(x => x.DisplayedName == SelectedWriteFunction);

                CurrentFunction = WriteFunction;

                UInt16[] ModbusWriteData;

                if (WriteFunction == Function.PresetMultipleRegisters ||
                    WriteFunction == Function.ForceMultipleCoils)
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
                    ModbusMessageType is ModbusTCP_Message ? false : CheckSum_Enable);                                

                ModbusOperationResult Result = 
                    await Model.Modbus.WriteRegister(
                        WriteFunction, 
                        Data,
                        ModbusMessageType);

                AddDataOnView(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = WriteFunction.DisplayedNumber,
                    Address = SelectedAddress,
                    ViewAddress = CreateViewAddress(SelectedAddress, ModbusWriteData.Length),
                    Data = ModbusWriteData,
                    ViewData = CreateViewData(ModbusWriteData)
                },
                Result.Details);
            }

            catch (ModbusException error)
            {
                ModbusErrorHandler(error);
            }

            catch (Exception error)
            {
                ModbusExceptionInfo? Info = error.InnerException as ModbusExceptionInfo;

                if (Info != null)
                {
                    AddDataOnView(null, Info.Details);
                }

                Message.Invoke("Возникла ошибка при нажатии на кнопку \"Записать\":\n\n" + error.Message, MessageType.Error);
            }
        }

        private async Task Modbus_Read()
        {
            ModbusOperationResult? Result;

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
                    ModbusMessageType is ModbusTCP_Message ? false : CheckSum_Enable);
                                
                Result = await Model.Modbus.ReadRegister(
                                ReadFunction,
                                Data,
                                ModbusMessageType);

                AddDataOnView(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = ReadFunction.DisplayedNumber,
                    Address = SelectedAddress,
                    ViewAddress = CreateViewAddress(SelectedAddress, Result.ReadedData == null ? 0 : Result.ReadedData.Length),
                    Data = Result.ReadedData,
                    ViewData = CreateViewData(Result.ReadedData)
                },
                Result.Details);
            }

            catch (ModbusException error)
            {
                ModbusErrorHandler(error);
            }

            catch (Exception error)
            {
                ModbusExceptionInfo? Info = error.InnerException as ModbusExceptionInfo;
                
                if (Info != null)
                {
                    AddDataOnView(null, Info.Details);
                }                

                Message.Invoke("Возникла ошибка при нажатии нажатии на кнопку \"Прочитать\": \n\n" + error.Message, MessageType.Error);
            }
        }

        private void ModbusErrorHandler(ModbusException error)
        {
            AddDataOnView(new ModbusDataDisplayed()
            {
                OperationID = PackageNumber,
                FuncNumber = CurrentFunction?.DisplayedNumber,
                Address = SelectedAddress,
                ViewAddress = CreateViewAddress(SelectedAddress, 1),
                Data = new UInt16[1],
                ViewData = "Ошибка Modbus.\nКод: " + error.ErrorCode.ToString()
            },
            error.Details);

            string Addition = String.Empty;

            if (CurrentFunction == Function.ForceSingleCoil && error.ErrorCode == 3)
            {
                Addition = "\n\nВ функции " + Function.ForceSingleCoil.DisplayedName + " используется логический тип данных.\n" +
                    "\nTrue - это 0xFF00" +
                    "\nFalse - это 0x0000";
            }

            Message.Invoke(
                "Ошибка Modbus.\n\n" +
                "Код функции: " + error.FunctionCode.ToString() + "\n" +
                "Код ошибки: " + error.ErrorCode.ToString() + "\n\n" +
                error.Message +
                Addition,
                MessageType.Error);
        }

        /*************************************************************************/
        //
        // Следующие три метода используются в ViewModel_Modbus_CycleMode
        //
        /*************************************************************************/

        private (string[], string) ParseData(byte[]? Data)
        {
            if (Data == null)
            {
                return (Array.Empty<string>(), string.Empty);
            }

            string[] DataBytes = new string[Data.Length];
            string StringForLog = string.Empty;

            for (int i = 0; i < Data.Length; i++)
            {
                DataBytes[i] = Data[i].ToString("X2");
                StringForLog += Data[i].ToString("X2") + "   ";
            }

            return (DataBytes, StringForLog);
        }

        public void AddDataOnView(ModbusDataDisplayed? Data, ModbusActionDetails? Details)
        {
            if (Details != null)
            {
                (string[] Bytes, string LogString) Request = ParseData(Details.RequestBytes);
                (string[] Bytes, string LogString) Response = ParseData(Details.ResponseBytes);

                // Добавление данных в "Последний запрос"

                RequestResponseItems.Clear();
                RequestResponseItems.AddRange(GetDataForLastRequest(Request.Bytes, Response.Bytes));

                // Добавление данных в "Лог"

                if (LogData != string.Empty)
                {
                    LogData += "\n\n";
                }

                if (Request.Bytes.Length > 0)
                {
                    LogData += Details.Request_ExecutionTime.ToString("HH : mm : ss . fff") + "   ->   " + Request.LogString;
                }

                if (Response.Bytes.Length > 0)
                {
                    if (Request.Bytes.Length > 0)
                    {
                        LogData += "\n";
                    }

                    LogData += Details.Response_ExecutionTime.ToString("HH : mm : ss . fff") + "   <-   " + Response.LogString;
                }
            }

            // Добавление строки в таблицу 

            if (Data != null)
            {
                DataInDataGrid.Add(Data);
            }            

            PackageNumber++;
        }

        private RequestResponseField_ItemData[] GetDataForLastRequest(string[] RequestBytes, string[] ResponseBytes)
        {
            int MaxLength = RequestBytes.Length > ResponseBytes.Length ? RequestBytes.Length : ResponseBytes.Length;

            RequestResponseField_ItemData[] Items = new RequestResponseField_ItemData[MaxLength];

            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = new RequestResponseField_ItemData();
                Items[i].ItemNumber = (i + 1).ToString();
            }

            for (int i = 0; i < RequestBytes.Length; i++)
            {
                Items[i].RequestDataType = i.ToString() + "X";
                Items[i].RequestData = RequestBytes[i];
            }

            for (int i = 0; i < ResponseBytes.Length; i++)
            {
                Items[i].ResponseDataType = i.ToString() + "Y";
                Items[i].ResponseData = ResponseBytes[i];
            }

            return Items;
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

        public static string CreateViewData(UInt16[]? ModbusData)
        {
            if (ModbusData == null)
            {
                return String.Empty;
            }
            
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
