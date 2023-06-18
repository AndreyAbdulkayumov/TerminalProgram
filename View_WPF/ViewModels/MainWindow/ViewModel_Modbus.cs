using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Core.Models;
using Core.Models.Modbus;
using Core.Models.Modbus.Message;
using System.Globalization;
using System.Reactive.Linq;

namespace View_WPF.ViewModels.MainWindow
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

    internal class ViewModel_Modbus : ReactiveObject
    {
        #region Properties

        private string _modbusMode_Name;

        public string ModbusMode_Name
        {
            get => _modbusMode_Name;
            set => this.RaiseAndSetIfChanged(ref _modbusMode_Name, value);
        }

        private readonly ObservableCollection<ModbusDataDisplayed> _dataDisplayedList =
            new ObservableCollection<ModbusDataDisplayed>();

        public ObservableCollection<ModbusDataDisplayed> DataDisplayedList
        {
            get => _dataDisplayedList;
        }

        private string _slaveID;

        public string SlaveID
        {
            get => _slaveID;
            set => this.RaiseAndSetIfChanged(ref _slaveID, value);
        }

        private bool _crc16_Enable;

        public bool CRC16_Enable
        {
            get => _crc16_Enable;
            set => this.RaiseAndSetIfChanged(ref _crc16_Enable, value);
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

        private string _address;

        public string Address
        {
            get => _address;
            set => this.RaiseAndSetIfChanged(ref _address, value);
        }

        private string _numberOfRegisters;

        public string NumberOfRegisters
        {
            get => _numberOfRegisters;
            set => this.RaiseAndSetIfChanged(ref _numberOfRegisters, value);
        }

        private string _writeData;

        public string WriteData
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

        private string _selectedReadFunction;

        public string SelectedReadFunction
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

        private string _selectedWriteFunction;

        public string SelectedWriteFunction
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
        private readonly Action<ModbusDataDisplayed> DataGrid_ScrollTo;

        private NumberStyles Address_NumberStyle = NumberStyles.HexNumber;
        private NumberStyles Data_NumberStyle = NumberStyles.HexNumber;

        private UInt16 PackageNumber = 0;

        private byte SelectedSlaveID = 0;
        private UInt16 SelectedAddress = 0;
        private UInt16 SelectedNumberOfRegisters = 1;

        private const UInt16 CRC_Polynom = 0xA001;
        private bool CRC_Enable = false;

        public ViewModel_Modbus(
            Action<string, MessageType> MessageBox,
            Action UI_Connected_Handler,
            Action UI_Disconnected_Handler,
            Action<ModbusDataDisplayed> UI_DataGrid_ScrollTo_Handler)
            
        {
            Message = MessageBox;

            SetUI_Connected = UI_Connected_Handler;
            SetUI_Disconnected = UI_Disconnected_Handler;

            DataGrid_ScrollTo = UI_DataGrid_ScrollTo_Handler;

            Model = ConnectedHost.Model;

            SetUI_Disconnected?.Invoke();

            ModbusMode_Name = "не определен";

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            CRC16_Enable = true;

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

            Command_ClearDataGrid = ReactiveCommand.Create(DataDisplayedList.Clear);
            Command_Write = ReactiveCommand.Create(Modbus_Write);
            Command_Read = ReactiveCommand.Create(Modbus_Read);

            this.WhenAnyValue(x => x.SlaveID)
                .Where(x => x != null)
                .Select(x => CheckNumber(x, NumberStyles.Number, out SelectedSlaveID))
                .Subscribe(x => SlaveID = x);

            this.WhenAnyValue(x => x.Address)
                .Where(x => x != null)
                .Select(x => CheckNumber(x, Address_NumberStyle, out SelectedAddress))
                .Subscribe(x => Address = x);

            this.WhenAnyValue(x => x.NumberOfRegisters)
                .Where(x => x != null)
                .Select(x => CheckNumber(x, NumberStyles.Number, out SelectedNumberOfRegisters))
                .Subscribe(x => NumberOfRegisters = x);
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            SetUI_Connected?.Invoke();

            ModbusMode_Name = "TEST";
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            SetUI_Disconnected?.Invoke();

            ModbusMode_Name = "не определен";
        }

        private void Modbus_Write()
        {
            try
            {
                if (Model.Modbus == null)
                {
                    Message?.Invoke("Не инициализирован Modbus клиент.", MessageType.Warning);
                    return;
                }

                //if (ModbusMessageType == null)
                //{
                //    Message?.Invoke("Не задан тип протокола Modbus.", MessageType.Warning);
                //    return;
                //}

                if (SlaveID == String.Empty)
                {
                    Message?.Invoke("Укажите Slave ID.", MessageType.Warning);
                    return;
                }

                if (Address == String.Empty)
                {
                    Message?.Invoke("Укажите адрес Modbus регистра.", MessageType.Warning);
                    return;
                }

                if (WriteData == String.Empty)
                {
                    Message?.Invoke("Укажите данные для записи в Modbus регистр.", MessageType.Warning);
                    return;
                }

                //MessageData Data = new WriteTypeMessage(
                //    SelectedSlaveID,
                //    SelectedAddress,
                //    ModbusWriteData,
                //    ModbusMessageType is ModbusTCP_Message ? false : CRC_Enable,
                //    CRC_Polynom);

                //Model.Modbus.WriteRegister(
                //    WriteFunction,
                //    Data,
                //    ModbusMessageType,
                //    out CommonResponse);

                //DataDisplayedList.Add(new ModbusDataDisplayed()
                //{
                //    OperationID = PackageNumber,
                //    FuncNumber = WriteFunction.DisplayedNumber,
                //    Address = SelectedAddress,
                //    ViewAddress = CreateViewAddress(SelectedAddress, ModbusWriteData.Length),
                //    Data = ModbusWriteData,
                //    ViewData = CreateViewData(ModbusWriteData)
                //});

                DataGrid_ScrollTo?.Invoke(DataDisplayedList.Last());

                PackageNumber++;
            }

            catch (ModbusException error)
            {
                ModbusErrorHandler(error);
            }

            catch (Exception error)
            {
                Message?.Invoke("Возникла ошибка при нажатии на кнопку \"Записать\":\n\n" +
                        error.Message + "\n\nКлиент не был отключен.", MessageType.Error);
            }
        }

        private void Modbus_Read()
        {

        }

        private void ModbusErrorHandler(ModbusException error)
        {
            //DataDisplayedList.Add(new ModbusDataDisplayed()
            //{
            //    OperationID = PackageNumber,
            //    FuncNumber = ReadFunction.DisplayedNumber,
            //    Address = SelectedAddress,
            //    ViewAddress = CreateViewAddress(SelectedAddress, 1),
            //    Data = new UInt16[1],
            //    ViewData = "Ошибка Modbus.\nКод: " + error.ErrorCode.ToString()
            //});

            DataGrid_ScrollTo?.Invoke(DataDisplayedList.Last());

            PackageNumber++;

            Message?.Invoke(
                "Ошибка Modbus.\n\n" +
                "Код функции: " + error.FunctionCode.ToString() + "\n" +
                "Код ошибки: " + error.ErrorCode.ToString() + "\n\n" +
                error.Message, 
                MessageType.Error);
        }

        private string CheckNumber(string StringNumber, NumberStyles Style, out Byte Number)
        {
            if (StringNumber == "")
            {
                Number = 0;
                return "";
            }

            string? str = StringNumber;

            if (Byte.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out Number) == false)
            {
                str = new string(StringNumber.Where(char.IsDigit).ToArray());

                Message?.Invoke("Ввод букв и знаков не допустим.\n\nДиапазон чисел от 0 до 255.", MessageType.Warning);

                if (str == null)
                {
                    return "";
                }

                StringNumber = str;
            }

            return StringNumber;
        }

        private string CheckNumber(string StringNumber, NumberStyles Style, out UInt16 Number)
        {
            if (StringNumber == "")
            {
                Number = 0;
                return "";
            }

            string? str = StringNumber;

            if (UInt16.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out Number) == false)
            {
                if (Style == NumberStyles.HexNumber)
                {
                    str = new string(StringNumber.Where(x => (char.IsDigit(x) || "ABCDEFabcdef".Contains(x))).ToArray());
                    Message?.Invoke("Допустим только ввод чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x0000 до 0xFFFF.", MessageType.Warning);
                }

                else
                {
                    str = new string(StringNumber.Where(char.IsDigit).ToArray());
                    Message?.Invoke("Допустим только ввод чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 65535.", MessageType.Warning);
                }
                

                if (str == null)
                {
                    return "";
                }

                StringNumber = str;
            }

            return StringNumber;
        }

        //private UInt16 CheckNumber(string StringNumber, NumberStyles Style)
        //{
        //    if (StringNumber == String.Empty)
        //    {
        //        return 0;
        //    }

        //    UInt16 Number;

        //    while (true)
        //    {
        //        if (UInt16.TryParse(StringNumber, Style, CultureInfo.InvariantCulture, out Number) == false)
        //        {
        //            StringNumber = StringNumber.Remove(StringNumber.Length - 1);
        //            //StringNumber.SelectionStart = StringNumber.Length;

        //            Message?.Invoke("Ввод букв и знаков не допустим.\n\nДиапазон чисел от 0 до 65 535 (0x0000 - 0xFFFF).", MessageType.Warning);
        //        }

        //        else
        //        {
        //            break;
        //        }

        //        if (StringNumber == String.Empty)
        //        {
        //            return 0;
        //        }
        //    }

        //    return Number;
        //}
    }
}
