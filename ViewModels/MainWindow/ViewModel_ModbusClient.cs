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

        #endregion

        #region Commands

        public ReactiveCommand<Unit, Unit> Command_Open_ModbusScanner { get; }
        public ReactiveCommand<Unit, Unit> Command_ClearData { get; }

        public ReactiveCommand<Unit, Unit> Command_Copy_Request { get; }
        public ReactiveCommand<Unit, Unit> Command_Copy_Response { get; }

        #endregion

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;

        public static ModbusMessage? ModbusMessageType { get; private set; }

        private UInt16 PackageNumber = 0;

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

            Mode_Normal_VM = new ViewModel_ModbusClient_Mode_Normal(MessageBox, Modbus_Write, Modbus_Read);
            Mode_Cycle_VM = new ViewModel_ModbusClient_Mode_Cycle(MessageBox, Modbus_Read);

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

        private async Task Modbus_Write(byte SlaveID, UInt16 Address, ModbusWriteFunction WriteFunction, UInt16[] ModbusWriteData, bool CheckSum_Enable)
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

        private async Task Modbus_Read(byte SlaveID, UInt16 Address, ModbusReadFunction ReadFunction, int NumberOfRegisters, bool CheckSum_Enable)
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

        private void ModbusErrorHandler(UInt16 Address, ModbusException error)
        {
            AddDataOnView(new ModbusDataDisplayed()
            {
                OperationID = PackageNumber,
                FuncNumber = CurrentFunction?.DisplayedNumber,
                Address = Address,
                ViewAddress = CreateViewAddress(Address, 1),
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
                AddDataOnTable?.Invoke(this, Data);
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
