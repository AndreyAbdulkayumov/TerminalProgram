using Core.Clients;
using Core.Models;
using Core.Models.Modbus;
using DynamicData;
using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;

namespace ViewModels.ModbusClient
{
    public class ModbusClient_Mode_Normal_VM : ReactiveObject
    {
        private bool ui_IsEnable = false;

        public bool UI_IsEnable
        {
            get => ui_IsEnable;
            set => this.RaiseAndSetIfChanged(ref ui_IsEnable, value);
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

        private ObservableCollection<ModbusClient_WriteData_VM> _writeDataCollection = new ObservableCollection<ModbusClient_WriteData_VM>();

        public ObservableCollection<ModbusClient_WriteData_VM> WriteDataCollection
        {
            get => _writeDataCollection;
            set => this.RaiseAndSetIfChanged(ref _writeDataCollection, value);
        }

        private bool _registerCanAdded;

        public bool RegisterCanAdded
        {
            get => _registerCanAdded;
            set => this.RaiseAndSetIfChanged(ref _registerCanAdded, value);
        }

        public ReactiveCommand<Unit, Unit> Command_Read { get; }
        public ReactiveCommand<Unit, Unit> Command_Write { get; }
        public ReactiveCommand<Unit, Unit> Command_AddRegister { get; }

        private readonly List<ushort> WriteBuffer = new List<ushort>();

        private NumberStyles _numberViewStyle;

        private byte _selectedSlaveID = 0;
        private ushort _selectedAddress = 0;
        private ushort _selectedNumberOfRegisters = 1;

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;


        public ModbusClient_Mode_Normal_VM(
            Action<string, MessageType> messageBox,
            Func<byte, ushort, ModbusWriteFunction, ushort[], bool, Task> modbus_Write,
            Func<byte, ushort, ModbusReadFunction, int, bool, Task> modbus_Read
            )
        {
            Message = messageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            /****************************************************/
            //
            // Первоначальная настройка UI
            //
            /****************************************************/

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

            Command_Read = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SlaveID == null || SlaveID == string.Empty)
                {
                    Message.Invoke("Укажите Slave ID.", MessageType.Warning);
                    return;
                }

                if (Address == null || Address == string.Empty)
                {
                    Message.Invoke("Укажите адрес Modbus регистра.", MessageType.Warning);
                    return;
                }

                if (NumberOfRegisters == null || NumberOfRegisters == string.Empty)
                {
                    Message.Invoke("Укажите количество регистров для чтения.", MessageType.Warning);
                    return;
                }

                if (_selectedNumberOfRegisters < 1)
                {
                    Message.Invoke("Сколько, сколько регистров вы хотите прочитать? :)", MessageType.Warning);
                    return;
                }

                ModbusReadFunction ReadFunction = Function.AllReadFunctions.Single(x => x.DisplayedName == SelectedReadFunction);

                await modbus_Read(_selectedSlaveID, _selectedAddress, ReadFunction, _selectedNumberOfRegisters, CheckSum_Enable);
            });

            Command_Write = ReactiveCommand.CreateFromTask(async () =>
            {
                if (SlaveID == null || SlaveID == string.Empty)
                {
                    Message.Invoke("Укажите Slave ID.", MessageType.Warning);
                    return;
                }

                if (Address == null || Address == string.Empty)
                {
                    Message.Invoke("Укажите адрес Modbus регистра.", MessageType.Warning);
                    return;
                }

                if (WriteDataCollection.Count == 0)
                {
                    Message.Invoke("Укажите данные для записи в Modbus регистры.", MessageType.Warning);
                    return;
                }

                ModbusWriteFunction WriteFunction = Function.AllWriteFunctions.Single(x => x.DisplayedName == SelectedWriteFunction);

                ushort[] ModbusWriteData;

                if (WriteFunction == Function.PresetMultipleRegisters ||
                    WriteFunction == Function.ForceMultipleCoils)
                {
                    ModbusWriteData = WriteDataCollection.Select(e => e.Data).ToArray();
                }

                else
                {
                    ModbusWriteData = [WriteDataCollection.First().Data];
                }

                await modbus_Write(_selectedSlaveID, _selectedAddress, WriteFunction, ModbusWriteData, CheckSum_Enable);
            });

            Command_AddRegister = ReactiveCommand.Create(() =>
            {
                WriteDataCollection.Add(new ModbusClient_WriteData_VM(
                    canRemove: true,
                    startAddressAddition: WriteDataCollection.Count,
                    data: 0,
                    dataFormat: "hex",
                    removeItemHandler: RemoveWriteDataItem
                    ));
            });

            this.WhenAnyValue(x => x.SlaveID)
                .WhereNotNull()
                .Select(x => StringValue.CheckNumber(x, _numberViewStyle, out _selectedSlaveID))
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
                .Select(x => StringValue.CheckNumber(x, _numberViewStyle, out _selectedAddress))
                .Subscribe(x => Address = x.ToUpper());

            this.WhenAnyValue(x => x.NumberOfRegisters)
                .WhereNotNull()
                .Select(x => StringValue.CheckNumber(x, NumberStyles.Number, out _selectedNumberOfRegisters))
                .Subscribe(x => NumberOfRegisters = x);

            this.WhenAnyValue(x => x.SelectedWriteFunction)
                .WhereNotNull()
                .Subscribe(x =>
                {
                    if (x == Function.ForceMultipleCoils.DisplayedName ||
                        x == Function.PresetMultipleRegisters.DisplayedName)
                    {
                        UpdateWriteDataCollection([
                            new ModbusClient_WriteData_VM(
                                canRemove: true,
                                startAddressAddition: 0,
                                data: WriteDataCollection.Count == 0 ? (ushort)0 : WriteDataCollection.First().Data,
                                dataFormat: WriteDataCollection.Count == 0 ? "hex" : WriteDataCollection.First().SelectedDataFormat,
                                removeItemHandler: RemoveWriteDataItem
                            )]);

                        RegisterCanAdded = true;
                        return;
                    }

                    if (WriteDataCollection.Count == 0)
                    {
                        UpdateWriteDataCollection([ 
                            new ModbusClient_WriteData_VM(
                                canRemove: false,
                                startAddressAddition: 0,
                                data: 0,
                                dataFormat: "hex",
                                removeItemHandler: RemoveWriteDataItem
                            )]);
                    }

                    else
                    {
                        UpdateWriteDataCollection([WriteDataCollection.First()]);
                    }                   

                    RegisterCanAdded = false;
                });

            this.WhenAnyValue(x => x.WriteData)
                .WhereNotNull()
                .Subscribe(x => WriteData = WriteData_TextChanged(x));
        }

        private void UpdateWriteDataCollection(ModbusClient_WriteData_VM[] newItems)
        {
            WriteDataCollection.Clear();
            WriteDataCollection.AddRange(newItems);
        }

        private void RemoveWriteDataItem(Guid selectedId)
        {
            var newCollection = WriteDataCollection.Where(e => e.Id != selectedId).ToArray();

            UpdateWriteDataCollection(newCollection);
        }

        public void SelectNumberFormat_Hex()
        {
            NumberFormat = ModbusClient_VM.ViewContent_NumberStyle_hex;
            _numberViewStyle = NumberStyles.HexNumber;

            if (SlaveID != null)
            {
                SlaveID = Convert.ToInt32(SlaveID).ToString("X");
            }

            if (Address != null)
            {
                Address = Convert.ToInt32(Address).ToString("X");
            }

            WriteData = ConvertDataTextIn(_numberViewStyle, WriteData);
        }

        private void SelectNumberFormat_Dec()
        {
            NumberFormat = ModbusClient_VM.ViewContent_NumberStyle_dec;
            _numberViewStyle = NumberStyles.Number;

            if (SlaveID != null)
            {
                SlaveID = int.Parse(SlaveID, NumberStyles.HexNumber).ToString();
            }

            if (Address != null)
            {
                Address = int.Parse(Address, NumberStyles.HexNumber).ToString();
            }

            WriteData = ConvertDataTextIn(_numberViewStyle, WriteData);
        }


        private string ConvertDataTextIn(NumberStyles style, string? text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            string[] SplitString = text.Split(' ');

            string[] Values = SplitString.Where(element => element != "").ToArray();

            string DataString = "";

            if (style == NumberStyles.Number)
            {
                foreach (string element in Values)
                {
                    DataString += int.Parse(element, NumberStyles.HexNumber).ToString() + " ";
                }
            }

            else if (style == NumberStyles.HexNumber)
            {
                foreach (string element in Values)
                {
                    DataString += Convert.ToInt32(element).ToString("X") + " ";
                }
            }

            return DataString;
        }

        private string WriteData_TextChanged(string enteredText)
        {
            try
            {
                if (SelectedWriteFunction == Function.PresetMultipleRegisters.DisplayedName ||
                    SelectedWriteFunction == Function.ForceMultipleCoils.DisplayedName)
                {
                    WriteBuffer.Clear();

                    string[] splitString = enteredText.Split(' ');

                    string[] values = splitString.Where(element => element != "").ToArray();

                    ushort buffer = 0;

                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = StringValue.CheckNumber(values[i], _numberViewStyle, out buffer);
                        WriteBuffer.Add(buffer);
                    }

                    // Если при второй итерации последний элемент в SplitString равен "",
                    // то в конце был пробел.
                    return string.Join(" ", values).ToUpper() + (splitString.Last() == "" ? " " : "");
                }

                else
                {
                    return StringValue.CheckNumber(WriteData, _numberViewStyle, out ushort _).ToUpper();
                }
            }

            catch (Exception error)
            {
                Message.Invoke("Возникла ошибка при изменении текста в поле \"Данные\":\n\n" +
                    error.Message, MessageType.Error);

                return string.Empty;
            }
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice is IPClient)
            {
                CheckSum_IsVisible = false;
            }

            else if (e.ConnectedDevice is SerialPortClient)
            {
                CheckSum_IsVisible = true;
            }

            else
            {
                return;
            }

            WriteBuffer.Clear();

            UI_IsEnable = true;
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            UI_IsEnable = false;

            CheckSum_IsVisible = true;
        }
    }
}
