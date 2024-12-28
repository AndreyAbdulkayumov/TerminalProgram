using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Core.Models;
using Core.Models.Modbus.Message;
using System.Reactive.Linq;
using Core.Clients;
using MessageBox_Core;
using DynamicData;
using ViewModels.ModbusClient.DataTypes;
using ViewModels.ModbusClient.ModbusRepresentations;
using Core.Models.Modbus.DataTypes;

namespace ViewModels.ModbusClient
{
    public class ModbusClient_VM : ReactiveObject
    {
        public static ModbusClient_VM? Instance { get; private set; }

        public const string ViewContent_NumberStyle_dec = "(dec)";
        public const string ViewContent_NumberStyle_hex = "(hex)";

        // Добавлять данные в DataGrid можно только из UI потока.
        // Поэтому используется событие, обработчик которого вызывается в behind code у файла с разметкой DataGrid.
        public static event EventHandler<ModbusDataDisplayed?>? AddDataOnTable;

        public event EventHandler<bool>? CheckSum_VisibilityChanged;

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

        private const string Modbus_TCP_Name = "Modbus TCP";
        private const string Modbus_RTU_Name = "Modbus RTU";
        private const string Modbus_ASCII_Name = "Modbus ASCII";
        private const string Modbus_RTU_over_TCP_Name = "Modbus RTU over TCP";
        private const string Modbus_ASCII_over_TCP_Name = "Modbus ASCII over TCP";


        private readonly ObservableCollection<string> _modbusTypes_SerialPortClient =
            new ObservableCollection<string>()
            {
                Modbus_RTU_Name, Modbus_ASCII_Name
            };

        private readonly ObservableCollection<string> _modbusTypes_IPClient =
            new ObservableCollection<string>()
            {
                Modbus_TCP_Name, Modbus_RTU_over_TCP_Name, Modbus_ASCII_over_TCP_Name
            };

        private ObservableCollection<string>? _availableModbusTypes;

        public ObservableCollection<string>? AvailableModbusTypes
        {
            get => _availableModbusTypes;
            set => this.RaiseAndSetIfChanged(ref _availableModbusTypes, value);
        }

        private string _selectedModbusType = string.Empty;

        public string SelectedModbusType
        {
            get => _selectedModbusType;
            set => this.RaiseAndSetIfChanged(ref _selectedModbusType, value);
        }

        private bool _buttonModbusScanner_IsVisible = true;

        public bool ButtonModbusScanner_IsVisible
        {
            get => _buttonModbusScanner_IsVisible;
            set => this.RaiseAndSetIfChanged(ref _buttonModbusScanner_IsVisible, value);
        }

        #endregion

        #region Representations Data

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

        private ObservableCollection<BinaryRepresentation_ItemData> _binaryRepresentationItems = new ObservableCollection<BinaryRepresentation_ItemData>();

        public ObservableCollection<BinaryRepresentation_ItemData> BinaryRepresentationItems
        {
            get => _binaryRepresentationItems;
            set => this.RaiseAndSetIfChanged(ref _binaryRepresentationItems, value);
        }

        private ObservableCollection<FloatRepresentation_ItemData> _floatRepresentationItems = new ObservableCollection<FloatRepresentation_ItemData>();

        public ObservableCollection<FloatRepresentation_ItemData> FloatRepresentationItems
        {
            get => _floatRepresentationItems;
            set => this.RaiseAndSetIfChanged(ref _floatRepresentationItems, value);
        }

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> Command_Open_ModbusScanner { get; }
        public ReactiveCommand<Unit, Unit> Command_ClearData { get; }

        public ReactiveCommand<Unit, Unit> Command_Copy_Request { get; }
        public ReactiveCommand<Unit, Unit> Command_Copy_Response { get; }

        #endregion

        public static ModbusMessage? ModbusMessageType { get; private set; }

        private readonly ConnectedHost Model;

        private readonly Func<Action, Task> _runInUIThread;

        private readonly IMessageBox _messageBox;     

        private ushort _packageNumber = 0;

        private ModbusFunction? _currentFunction;

        private readonly ModbusClient_Mode_Normal_VM Mode_Normal_VM;
        private readonly ModbusClient_Mode_Cycle_VM Mode_Cycle_VM;

        private readonly Func<string, Task> _copyToClipboard;

        public ModbusClient_VM(
            Func<Action, Task> runInUIThread,
            Func<Task> open_ModbusScanner,
            IMessageBox messageBox,
            Func<string, Task> copyToClipboard
            )
        {
            _runInUIThread = runInUIThread;

            _messageBox = messageBox;

            _copyToClipboard = copyToClipboard;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Mode_Normal_VM = new ModbusClient_Mode_Normal_VM(messageBox, Modbus_Write, Modbus_Read);
            Mode_Normal_VM.Subscribe(this);

            Mode_Cycle_VM = new ModbusClient_Mode_Cycle_VM(messageBox, Modbus_Read);       
            Mode_Cycle_VM.Subscribe(this);

            Instance = this;

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

                await copyToClipboard(Data);
            });
            Command_Copy_Request.ThrownExceptions.Subscribe(error => _messageBox.Show("Ошибка копирования запроса в буфер обмена.\n\n" + error.Message, MessageType.Error));

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

                await copyToClipboard(Data);
            });
            Command_Copy_Response.ThrownExceptions.Subscribe(error => _messageBox.Show("Ошибка копирования ответа в буфер обмена.\n\n" + error.Message, MessageType.Error));

            Command_Open_ModbusScanner = ReactiveCommand.CreateFromTask(open_ModbusScanner);

            Command_ClearData = ReactiveCommand.Create(() =>
            {
                AddDataOnTable?.Invoke(this, null);

                RequestResponseItems.Clear();
                LogData = string.Empty;
            });
            Command_ClearData.ThrownExceptions.Subscribe(error => _messageBox.Show("Ошибка очистки данных.\n\n" + error.Message, MessageType.Error));

            this.WhenAnyValue(x => x.IsCycleMode)
                .Subscribe(_ =>
                {
                    if (!IsCycleMode)
                    {
                        Mode_Cycle_VM.StopPolling();
                    }

                    CurrentModeViewModel = IsCycleMode ? Mode_Cycle_VM : Mode_Normal_VM;
                });

            this.WhenAnyValue(x => x.SelectedModbusType)
                .WhereNotNull()
                .Subscribe(x =>
                {
                    if (Model.HostIsConnect)
                    {
                        SetCheckSumVisiblity();

                        if (SelectedModbusType == Modbus_TCP_Name)
                        {
                            ModbusMessageType = new ModbusTCP_Message();
                            return;
                        }

                        if (SelectedModbusType == Modbus_RTU_Name || 
                            SelectedModbusType == Modbus_RTU_over_TCP_Name)
                        {
                            ModbusMessageType = new ModbusRTU_Message();
                            return;
                        }

                        if (SelectedModbusType == Modbus_ASCII_Name ||
                            SelectedModbusType == Modbus_ASCII_over_TCP_Name)
                        {
                            ModbusMessageType = new ModbusASCII_Message();
                            return;
                        }

                        _messageBox.Show("Задан неизвестный тип Modbus протокола: " + SelectedModbusType, MessageType.Error);
                    }
                });
        }

        private void SetCheckSumVisiblity()
        {
            bool isVisible = !Model.HostIsConnect || SelectedModbusType != Modbus_TCP_Name;

            CheckSum_VisibilityChanged?.Invoke(this, isVisible);
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice is IPClient)
            {
                AvailableModbusTypes = _modbusTypes_IPClient;

                ButtonModbusScanner_IsVisible = false;
            }

            else if (e.ConnectedDevice is SerialPortClient)
            {
                AvailableModbusTypes = _modbusTypes_SerialPortClient;

                ButtonModbusScanner_IsVisible = true;
            }

            else
            {
                _messageBox.Show("Задан неизвестный тип подключения.", MessageType.Error);
                return;
            }

            SelectedModbusType = AvailableModbusTypes.Contains(SelectedModbusType) ? SelectedModbusType : AvailableModbusTypes.First();

            SetCheckSumVisiblity();

            UI_IsEnable = true;
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            UI_IsEnable = false;

            ButtonModbusScanner_IsVisible = true;

            SetCheckSumVisiblity();

            _packageNumber = 0;
        }

        public async Task Modbus_Read(byte slaveID, ushort address, ModbusReadFunction readFunction, int numberOfRegisters, bool checkSum_Enable)
        {
            try
            {
                if (Model.HostIsConnect == false)
                {
                    throw new Exception("Modbus клиент отключен.");
                }

                if (Model.Modbus == null)
                {
                    throw new Exception("Не инициализирован Modbus клиент.");
                }

                if (ModbusMessageType == null)
                {
                    throw new Exception("Не задан тип протокола Modbus.");
                }

                if (numberOfRegisters < 1)
                {
                    throw new Exception("Сколько, сколько регистров вы хотите прочитать? :)");
                }

                _currentFunction = readFunction;

                MessageData data = new ReadTypeMessage(
                    slaveID,
                    address,
                    numberOfRegisters,
                    ModbusMessageType is ModbusTCP_Message ? false : checkSum_Enable);

                ModbusOperationResult? result = await Model.Modbus.ReadRegister(
                                readFunction,
                                data,
                                ModbusMessageType);

                AddDataOnView(new ModbusDataDisplayed()
                {
                    OperationID = _packageNumber,
                    FuncNumber = readFunction.DisplayedNumber,
                    Address = address,
                    ViewAddress = CreateViewAddress(address, result.ReadedData == null ? 0 : numberOfRegisters),
                    Data = result.ReadedData,
                    ViewData = CreateViewData(result.ReadedData, readFunction, numberOfRegisters)
                },
                result.Details);
            }

            catch (ModbusException error)
            {
                ModbusErrorHandler(address, error);
            }

            catch (Exception error)
            {
                var info = error.InnerException as ModbusExceptionInfo;

                if (info != null)
                {
                    AddDataOnView(null, info.Details);
                }

                throw new Exception(error.Message);
            }
        }

        public async Task Modbus_Write(byte slaveID, ushort address, ModbusWriteFunction writeFunction, byte[]? modbusWriteData, int numberOfRegisters, bool checkSum_Enable)
        {
            try
            {
                if (Model.HostIsConnect == false)
                {
                    throw new Exception("Modbus клиент отключен.");
                }

                if (Model.Modbus == null)
                {
                    throw new Exception("Не инициализирован Modbus клиент.");
                }

                if (ModbusMessageType == null)
                {
                    throw new Exception("Не задан тип протокола Modbus.");
                }

                if (modbusWriteData == null || modbusWriteData.Length == 0)
                {
                    throw new Exception("Укажите данные для записи.");
                }

                _currentFunction = writeFunction;

                MessageData data = new WriteTypeMessage(
                    slaveID,
                    address,
                    modbusWriteData,
                    numberOfRegisters,
                    ModbusMessageType is ModbusTCP_Message ? false : checkSum_Enable);

                ModbusOperationResult result = await Model.Modbus.WriteRegister(
                    writeFunction,
                    data,
                    ModbusMessageType);

                AddDataOnView(new ModbusDataDisplayed()
                {
                    OperationID = _packageNumber,
                    FuncNumber = writeFunction.DisplayedNumber,
                    Address = address,
                    ViewAddress = CreateViewAddress(address, numberOfRegisters),
                    Data = modbusWriteData,
                    ViewData = CreateViewData(modbusWriteData, writeFunction, numberOfRegisters)
                },
                result.Details);
            }

            catch (ModbusException error)
            {
                ModbusErrorHandler(address, error);
            }

            catch (Exception error)
            {
                var Info = error.InnerException as ModbusExceptionInfo;

                if (Info != null)
                {
                    AddDataOnView(null, Info.Details);
                }

                throw new Exception(error.Message);
            }
        }

        private void ModbusErrorHandler(ushort address, ModbusException error)
        {
            AddDataOnView(new ModbusDataDisplayed()
            {
                OperationID = _packageNumber,
                FuncNumber = _currentFunction?.DisplayedNumber,
                Address = address,
                ViewAddress = CreateViewAddress(address, 1),
                Data = Array.Empty<byte>(),
                ViewData = $"Ошибка Modbus.\nКод: {error.ErrorCode.ToString()}"
            },
            error.Details);

            string addition = string.Empty;

            if (_currentFunction == Function.ForceSingleCoil && error.ErrorCode == 3)
            {
                addition = "\n\nВ функции " + Function.ForceSingleCoil.DisplayedName + " используется логический тип данных.\n" +
                    "\nTrue - это 0xFF00" +
                    "\nFalse - это 0x0000";
            }

            throw new Exception(
                "Ошибка Modbus.\n\n" +
                $"Код функции: {error.FunctionCode.ToString()}\n" +
                $"Код ошибки: {error.ErrorCode.ToString()}\n\n" +
                error.Message +
                addition);
        }

        private (string[], string) ParseData(byte[]? data)
        {
            if (data == null)
            {
                return (Array.Empty<string>(), string.Empty);
            }

            var dataBytes = new string[data.Length];
            string stringForLog = string.Empty;

            for (int i = 0; i < data.Length; i++)
            {
                dataBytes[i] = data[i].ToString("X2");
                stringForLog += data[i].ToString("X2") + "   ";
            }

            return (dataBytes, stringForLog);
        }

        /*************************************************************************/
        //
        // Следующие три метода используются в Modbus_Mode_Cycle_VM
        //
        /*************************************************************************/

        public void AddDataOnView(ModbusDataDisplayed? data, ModbusActionDetails? details)
        {
            if (data != null)
            {
                AddDataOnTable?.Invoke(this, data);

                _runInUIThread.Invoke(() =>
                {
                    BinaryRepresentationItems.Clear();

                    var binaryItems = BinaryRepresentation.GetData(data, _messageBox, _copyToClipboard);

                    if (binaryItems != null)
                    {
                        BinaryRepresentationItems.AddRange(binaryItems);
                    }                    
                });

                _runInUIThread.Invoke(() =>
                {
                    FloatRepresentationItems.Clear();

                    var floatItems = FloatRepresentation.GetData(data);

                    if (floatItems != null)
                    {
                        FloatRepresentationItems.AddRange(floatItems);
                    }
                });
            }

            if (details != null)
            {
                (string[] Bytes, string LogString) request = ParseData(details.RequestBytes);
                (string[] Bytes, string LogString) response = ParseData(details.ResponseBytes);

                _runInUIThread.Invoke(() =>
                {
                    RequestResponseItems.Clear();
                    RequestResponseItems.AddRange(LastRequestRepresentation.GetData(request.Bytes, response.Bytes));
                });

                if (LogData != string.Empty)
                {
                    LogData += "\n\n";
                }

                LogData += LogRepresentation.GetData(
                    details.Request_ExecutionTime, request.LogString, 
                    details.Response_ExecutionTime, response.LogString);
            }

            _packageNumber++;
        }

        public static string CreateViewAddress(ushort startAddress, int numberOfRegisters)
        {
            string displayedString = string.Empty;

            ushort currentAddress = startAddress;

            for (int i = 0; i < numberOfRegisters; i++)
            {
                displayedString += $"0x{currentAddress.ToString("X")} ({currentAddress.ToString()})";

                if (i != numberOfRegisters - 1)
                {
                    displayedString += "\n";
                }

                currentAddress++;
            }

            return displayedString;
        }

        public static string CreateViewData(byte[]? modbusData, ModbusFunction function, int numberOfRegisters)
        {
            if (modbusData == null)
            {
                return string.Empty;
            }

            if (function.Number == Function.ForceMultipleCoils.Number || 
                function.Number == Function.ReadCoilStatus.Number ||
                function.Number == Function.ReadDiscreteInputs.Number)
            {
                return GetViewData_FromBytes(modbusData, numberOfRegisters);
            }

            return GetViewData_FromWords(modbusData);           
        }

        private static string GetViewData_FromWords(byte[] modbusData)
        {
            string displayedString = string.Empty;

            UInt16 temp;

            for (int i = 0; i < modbusData.Length - 1; i += 2)
            {
                temp = (UInt16)((modbusData[i + 1] << 8) | modbusData[i]);

                displayedString += $"0x{temp.ToString("X")} ({temp.ToString()})\n";
            }

            // Обработка последнего байта, если длина массива нечетная
            if (modbusData.Length % 2 != 0)
            {
                temp = modbusData.Last();

                displayedString += $"0x{temp.ToString("X")} ({temp.ToString()})\n";
            }

            displayedString = displayedString.TrimEnd('\n');

            return displayedString;
        }

        private static string GetViewData_FromBytes(byte[] modbusData, int numberOfRegisters)
        {
            string displayedString = string.Empty;

            int registerCounter = 0;

            foreach (byte element in modbusData) 
            {
                for (int i = 0; i < 8; i++)
                {
                    if (registerCounter == numberOfRegisters)
                    {
                        break;
                    }

                    displayedString += (element & (1 << (i))) != 0 ? "1\n" : "0\n";
                    registerCounter++;                    
                }
            }

            displayedString = displayedString.TrimEnd('\n');

            return displayedString;
        }
    }
}
