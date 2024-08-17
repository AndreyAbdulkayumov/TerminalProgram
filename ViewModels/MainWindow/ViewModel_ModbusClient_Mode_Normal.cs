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

namespace ViewModels.MainWindow
{
    public class ViewModel_ModbusClient_Mode_Normal : ReactiveObject
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

        private ObservableCollection<ViewModel_ModbusClient_WriteData> _writeDataCollection = new ObservableCollection<ViewModel_ModbusClient_WriteData>();

        public ObservableCollection<ViewModel_ModbusClient_WriteData> WriteDataCollection
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

        private readonly List<UInt16> WriteBuffer = new List<UInt16>();

        private NumberStyles NumberViewStyle;

        private byte SelectedSlaveID = 0;
        private UInt16 SelectedAddress = 0;
        private UInt16 SelectedNumberOfRegisters = 1;

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;


        public ViewModel_ModbusClient_Mode_Normal(
            Action<string, MessageType> MessageBox,
            Func<byte, UInt16, ModbusWriteFunction, UInt16[], bool, Task> Modbus_Write,
            Func<byte, UInt16, ModbusReadFunction, int, bool, Task> Modbus_Read
            )
        {
            Message = MessageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            /****************************************************/
            //
            // Первоначальная настройка UI
            //
            /****************************************************/

            WriteDataCollection.Add(new ViewModel_ModbusClient_WriteData(
                Address: 16248,
                Data: 26529,
                DataFormat: "bin"
                ));

            WriteDataCollection.Add(new ViewModel_ModbusClient_WriteData(
                Address: 16249,
                Data: 65457,
                DataFormat: "hex"
                ));

            WriteDataCollection.Add(new ViewModel_ModbusClient_WriteData(
                Address: 16250,
                Data: 45865,
                DataFormat: "dec"
                ));

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

                await Modbus_Read(SelectedSlaveID, SelectedAddress, ReadFunction, SelectedNumberOfRegisters, CheckSum_Enable);
            });

            Command_Write = ReactiveCommand.CreateFromTask(async () =>
            {
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

                await Modbus_Write(SelectedSlaveID, SelectedAddress, WriteFunction, ModbusWriteData, CheckSum_Enable);
            });

            Command_AddRegister = ReactiveCommand.Create(() =>
            {
                WriteDataCollection.Add(new ViewModel_ModbusClient_WriteData(
                    Address:(ushort)(WriteDataCollection.Last().Address + 1),
                    Data: 0,
                    DataFormat: "hex"
                    ));
            }); 

            this.WhenAnyValue(x => x.SlaveID)
                .WhereNotNull()
                .Select(x => StringValue.CheckNumber(x, NumberViewStyle, out SelectedSlaveID))
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

            //this.WhenAnyValue(x => x.SelectedWriteFunction)
            //    .WhereNotNull()
            //    .Where(x => x != String.Empty)
            //    .Subscribe(_ => WriteData = String.Empty);

            this.WhenAnyValue(x => x.SelectedWriteFunction)
                .WhereNotNull()
                .Subscribe(x =>
                {
                    if (x == Function.ForceMultipleCoils.DisplayedName ||
                        x == Function.PresetMultipleRegisters.DisplayedName)
                    {
                        RegisterCanAdded = true;
                    }

                    else
                    {
                        RegisterCanAdded = false;
                    }
                });

            this.WhenAnyValue(x => x.WriteData)
                .WhereNotNull()
                .Subscribe(x => WriteData = WriteData_TextChanged(x));
        }

        public void SelectNumberFormat_Hex()
        {
            NumberFormat = ViewModel_ModbusClient.ViewContent_NumberStyle_hex;
            NumberViewStyle = NumberStyles.HexNumber;

            if (SlaveID != null)
            {
                SlaveID = Convert.ToInt32(SlaveID).ToString("X");
            }

            if (Address != null)
            {
                Address = Convert.ToInt32(Address).ToString("X");
            }

            WriteData = ConvertDataTextIn(NumberViewStyle, WriteData);

            UpdateWriteDataCollection(NumberFormat, NumberViewStyle);
        }

        private void SelectNumberFormat_Dec()
        {
            NumberFormat = ViewModel_ModbusClient.ViewContent_NumberStyle_dec;
            NumberViewStyle = NumberStyles.Number;

            if (SlaveID != null)
            {
                SlaveID = Int32.Parse(SlaveID, NumberStyles.HexNumber).ToString();
            }

            if (Address != null)
            {
                Address = Int32.Parse(Address, NumberStyles.HexNumber).ToString();
            }

            WriteData = ConvertDataTextIn(NumberViewStyle, WriteData);

            UpdateWriteDataCollection(NumberFormat, NumberViewStyle);
        }

        private void UpdateWriteDataCollection(string AddressNumberFormat, NumberStyles AddressViewStyle)
        {
            ViewModel_ModbusClient_WriteData[] NewWriteDataCollection = WriteDataCollection.ToArray();

            foreach (ViewModel_ModbusClient_WriteData element in NewWriteDataCollection)
            {
                element.ViewAddress = ViewModel_ModbusClient_WriteData.ConvertNumberToString(element.Address, AddressViewStyle);
                element.AddressNumberFormat = AddressNumberFormat;
            }

            WriteDataCollection.Clear();
            WriteDataCollection.AddRange(NewWriteDataCollection);
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
