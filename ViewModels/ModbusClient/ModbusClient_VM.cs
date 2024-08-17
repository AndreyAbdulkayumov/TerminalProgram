using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Core.Models;
using Core.Models.Modbus;
using Core.Models.Modbus.Message;
using System.Reactive.Linq;
using Core.Clients;
using MessageBox_Core;
using DynamicData;
using ViewModels.ModbusClient.DataTypes;
using ViewModels.ModbusClient.ModbusRepresentations;

namespace ViewModels.ModbusClient
{
    public class ModbusClient_VM : ReactiveObject
    {
        public static string ViewContent_NumberStyle_dec = "(dec)";
        public const string ViewContent_NumberStyle_hex = "(hex)";

        // Добавлять данные в DataGrid можно только из UI потока.
        // Поэтому используется событие, обработчик которого вызывается в behind code у файла с разметкой DataGrid.
        public static event EventHandler<ModbusDataDisplayed?>? AddDataOnTable;

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

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> Command_Open_ModbusScanner { get; }
        public ReactiveCommand<Unit, Unit> Command_ClearData { get; }

        public ReactiveCommand<Unit, Unit> Command_Copy_Request { get; }
        public ReactiveCommand<Unit, Unit> Command_Copy_Response { get; }

        public ReactiveCommand<Unit, Unit> Command_Copy_BinaryWord { get; }

        #endregion

        private readonly ConnectedHost Model;

        private readonly Func<Action, Task> RunInUIThread;

        private readonly Action<string, MessageType> Message;

        public static ModbusMessage? ModbusMessageType { get; private set; }

        private ushort PackageNumber = 0;

        private ModbusFunction? CurrentFunction;

        private readonly ModbusClient_Mode_Normal_VM Mode_Normal_VM;
        private readonly ModbusClient_Mode_Cycle_VM Mode_Cycle_VM;


        public ModbusClient_VM(
            Func<Action, Task> RunInUIThread,
            Func<Task> Open_ModbusScanner,
            Action<string, MessageType> MessageBox,
            Func<string, Task> CopyToClipboard
            )
        {
            this.RunInUIThread = RunInUIThread;

            Message = MessageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Mode_Normal_VM = new ModbusClient_Mode_Normal_VM(MessageBox, Modbus_Write, Modbus_Read);
            Mode_Cycle_VM = new ModbusClient_Mode_Cycle_VM(MessageBox, Modbus_Read);

            /****************************************************/
            //
            // Первоначальная настройка UI
            //
            /****************************************************/

            Selected_Modbus_RTU_ASCII = Modbus_RTU_ASCII.First();

            ModbusMode_Name = ModbusMode_Name_Default;

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

            Command_Copy_BinaryWord = ReactiveCommand.CreateFromTask(async () =>
            {
                await CopyToClipboard("Test Data");
            });
            Command_Copy_BinaryWord.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка копирования ответа в буфер обмена.\n\n" + error.Message, MessageType.Error));

            Command_Open_ModbusScanner = ReactiveCommand.CreateFromTask(Open_ModbusScanner);

            Command_ClearData = ReactiveCommand.Create(() =>
            {
                AddDataOnTable?.Invoke(this, null);

                RequestResponseItems.Clear();
                LogData = string.Empty;
            });
            Command_ClearData.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка очистки данных.\n\n" + error.Message, MessageType.Error));

            this.WhenAnyValue(x => x.IsCycleMode)
                .Subscribe(_ =>
                {
                    if (!IsCycleMode)
                    {
                        Mode_Cycle_VM.Start_Stop_Handler(false);
                    }

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
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice is IPClient)
            {
                ModbusMessageType = new ModbusTCP_Message();

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

                Connection_IsSerialPort = true;
            }

            else
            {
                Message.Invoke("Задан неизвестный тип подключения.", MessageType.Error);
                return;
            }

            ModbusMode_Name = ModbusMessageType.ProtocolName;

            UI_IsEnable = true;
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            UI_IsEnable = false;

            Connection_IsSerialPort = false;

            ModbusMode_Name = ModbusMode_Name_Default;

            PackageNumber = 0;
        }

        private async Task Modbus_Write(byte SlaveID, ushort Address, ModbusWriteFunction WriteFunction, ushort[] ModbusWriteData, bool CheckSum_Enable)
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

                CurrentFunction = WriteFunction;

                MessageData Data = new WriteTypeMessage(
                    SlaveID,
                    Address,
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
                    Address = Address,
                    ViewAddress = CreateViewAddress(Address, ModbusWriteData.Length),
                    Data = ModbusWriteData,
                    ViewData = CreateViewData(ModbusWriteData)
                },
                Result.Details);
            }

            catch (ModbusException error)
            {
                ModbusErrorHandler(Address, error);
            }

            catch (Exception error)
            {
                ModbusExceptionInfo? Info = error.InnerException as ModbusExceptionInfo;

                if (Info != null)
                {
                    AddDataOnView(null, Info.Details);
                }

                Message.Invoke("Возникла ошибка при попытке записи:\n\n" + error.Message, MessageType.Error);
            }
        }

        private async Task Modbus_Read(byte SlaveID, ushort Address, ModbusReadFunction ReadFunction, int NumberOfRegisters, bool CheckSum_Enable)
        {
            ModbusOperationResult? Result;

            byte[] RequestBytes = Array.Empty<byte>();
            byte[] ResponseBytes = Array.Empty<byte>();

            try
            {
                if (Model.HostIsConnect == false)
                {
                    Message.Invoke("Modbus клиент отключен.", MessageType.Error);
                    return;
                }

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

                CurrentFunction = ReadFunction;

                MessageData Data = new ReadTypeMessage(
                    SlaveID,
                    Address,
                    NumberOfRegisters,
                    ModbusMessageType is ModbusTCP_Message ? false : CheckSum_Enable);

                Result = await Model.Modbus.ReadRegister(
                                ReadFunction,
                                Data,
                                ModbusMessageType);

                AddDataOnView(new ModbusDataDisplayed()
                {
                    OperationID = PackageNumber,
                    FuncNumber = ReadFunction.DisplayedNumber,
                    Address = Address,
                    ViewAddress = CreateViewAddress(Address, Result.ReadedData == null ? 0 : Result.ReadedData.Length),
                    Data = Result.ReadedData,
                    ViewData = CreateViewData(Result.ReadedData)
                },
                Result.Details);
            }

            catch (ModbusException error)
            {
                ModbusErrorHandler(Address, error);
            }

            catch (Exception error)
            {
                ModbusExceptionInfo? Info = error.InnerException as ModbusExceptionInfo;

                if (Info != null)
                {
                    AddDataOnView(null, Info.Details);
                }

                Message.Invoke("Возникла ошибка при попытке чтения: \n\n" + error.Message, MessageType.Error);
            }
        }

        private void ModbusErrorHandler(ushort Address, ModbusException error)
        {
            AddDataOnView(new ModbusDataDisplayed()
            {
                OperationID = PackageNumber,
                FuncNumber = CurrentFunction?.DisplayedNumber,
                Address = Address,
                ViewAddress = CreateViewAddress(Address, 1),
                Data = new ushort[1],
                ViewData = "Ошибка Modbus.\nКод: " + error.ErrorCode.ToString()
            },
            error.Details);

            string Addition = string.Empty;

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

        /*************************************************************************/
        //
        // Следующие три метода используются в Modbus_Mode_Cycle_VM
        //
        /*************************************************************************/

        public void AddDataOnView(ModbusDataDisplayed? Data, ModbusActionDetails? Details)
        {
            if (Data != null)
            {
                AddDataOnTable?.Invoke(this, Data);

                RunInUIThread.Invoke(() =>
                {
                    BinaryRepresentationItems.Clear();
                    BinaryRepresentationItems.AddRange(BinaryRepresentation.GetData(Data));
                });                
            }

            if (Details != null)
            {
                (string[] Bytes, string LogString) Request = ParseData(Details.RequestBytes);
                (string[] Bytes, string LogString) Response = ParseData(Details.ResponseBytes);

                RunInUIThread.Invoke(() =>
                {
                    RequestResponseItems.Clear();
                    RequestResponseItems.AddRange(LastRequestRepresentation.GetData(Request.Bytes, Response.Bytes));
                });

                if (LogData != string.Empty)
                {
                    LogData += "\n\n";
                }

                LogData += LogRepresentation.GetData(
                    Details.Request_ExecutionTime, Request.LogString, 
                    Details.Response_ExecutionTime, Response.LogString);
            }

            PackageNumber++;
        }

        public static string CreateViewAddress(ushort StartAddress, int NumberOfRegisters)
        {
            string DisplayedString = string.Empty;

            ushort CurrentAddress = StartAddress;

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

        public static string CreateViewData(ushort[]? ModbusData)
        {
            if (ModbusData == null)
            {
                return string.Empty;
            }

            string DisplayedString = string.Empty;

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
