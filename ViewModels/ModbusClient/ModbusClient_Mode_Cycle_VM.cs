﻿using Core.Models;
using ReactiveUI;
using System.Reactive;
using System.Collections.ObjectModel;
using Core.Models.Modbus;
using System.Globalization;
using System.Reactive.Linq;
using MessageBox_Core;
using Core.Clients;

namespace ViewModels.ModbusClient
{
    public class ModbusClient_Mode_Cycle_VM : ReactiveObject
    {
        private bool _ui_IsEnable = false;

        public bool UI_IsEnable
        {
            get => _ui_IsEnable;
            set => this.RaiseAndSetIfChanged(ref _ui_IsEnable, value);
        }

        private string? _slaveID;

        public string? SlaveID
        {
            get => _slaveID;
            set => this.RaiseAndSetIfChanged(ref _slaveID, value);
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

        private int _period_ms;

        public int Period_ms
        {
            get => _period_ms;
            set => this.RaiseAndSetIfChanged(ref _period_ms, value);
        }

        private string? _address;

        public string? Address
        {
            get => _address;
            set => this.RaiseAndSetIfChanged(ref _address, value);
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

        private ushort _numberOfRegisters = 1;

        public ushort NumberOfRegisters
        {
            get => _numberOfRegisters;
            set => this.RaiseAndSetIfChanged(ref _numberOfRegisters, value);
        }

        #region Button

        private const string Button_Content_Start = "Начать опрос";
        private const string Button_Content_Stop = "Остановить опрос";

        private string _button_Content = Button_Content_Start;

        public string Button_Content
        {
            get => _button_Content;
            set => this.RaiseAndSetIfChanged(ref _button_Content, value);
        }

        public ReactiveCommand<Unit, Unit> Command_Start_Stop_Polling { get; }

        #endregion


        private bool _isStart = false;

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;

        private NumberStyles _numberViewStyle;

        private byte _selectedSlaveID = 0;
        private ushort _selectedAddress = 0;

        private bool _checkSum_IsEnable;

        // Время в мс. взято с запасом.
        // Это время нужно для совместимости с методом Receive() из класса SerialPortClient
        private const int TimeForReadHandler = 100;

        private readonly Func<byte, ushort, ModbusReadFunction, int, bool, Task> Modbus_Read;      


        public ModbusClient_Mode_Cycle_VM(
            Action<string, MessageType> messageBox,
            Func<byte, ushort, ModbusReadFunction, int, bool, Task> modbus_Read
            )
        {
            Message = messageBox;

            Modbus_Read = modbus_Read;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Model.Modbus.Model_ErrorInCycleMode += Modbus_Model_ErrorInCycleMode;

            Period_ms = 600;

            Command_Start_Stop_Polling = ReactiveCommand.Create(() =>
            {
                Start_Stop_Handler(!_isStart);
            });
            Command_Start_Stop_Polling.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            foreach (ModbusReadFunction element in Function.AllReadFunctions)
            {
                ReadFunctions.Add(element.DisplayedName);
            }

            SelectedReadFunction = Function.ReadInputRegisters.DisplayedName;

            SelectedNumberFormat_Hex = true;

            this.WhenAnyValue(x => x.SlaveID)
                .WhereNotNull()
                .Select(x => StringValue.CheckNumber(x, _numberViewStyle, out _selectedSlaveID))
                .Subscribe(x => SlaveID = x);

            this.WhenAnyValue(x => x.Address)
                .WhereNotNull()
                .Select(x => StringValue.CheckNumber(x, _numberViewStyle, out _selectedAddress))
                .Subscribe(x => Address = x.ToUpper());

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
        }

        public void SourceWindowClosingAction()
        {
            Model.Modbus.CycleMode_Stop();
            Model.Modbus.Model_ErrorInCycleMode -= Modbus_Model_ErrorInCycleMode;
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            UI_IsEnable = true;

            _checkSum_IsEnable = e.ConnectedDevice is SerialPortClient;
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            UI_IsEnable = false;

            Start_Stop_Handler(false);
        }

        private void Modbus_Model_ErrorInCycleMode(object? sender, string e)
        {
            Model.Modbus.CycleMode_Stop();

            if (_isStart)
            {
                Button_Content = Button_Content_Start;
                _isStart = false;
            }

            Message.Invoke(e, MessageType.Error);
        }

        public void SelectNumberFormat_Hex()
        {
            _numberViewStyle = NumberStyles.HexNumber;

            if (Address != null)
            {
                Address = Convert.ToInt32(Address).ToString("X");
            }
        }

        private void SelectNumberFormat_Dec()
        {
            _numberViewStyle = NumberStyles.Number;

            if (Address != null)
            {
                Address = int.Parse(Address, NumberStyles.HexNumber).ToString();
            }
        }

        public void Start_Stop_Handler(bool startPolling)
        {
            if (startPolling)
            {
                StartAction();
                Button_Content = Button_Content_Stop;
            }

            else
            {
                Model.Modbus.CycleMode_Stop();
                Button_Content = Button_Content_Start;
            }

            _isStart = startPolling;
        }

        private void StartAction()
        {
            if (Address == null || Address == string.Empty)
            {
                Message.Invoke("Укажите адрес Modbus регистра.", MessageType.Warning);
                return;
            }

            if (NumberOfRegisters == 0)
            {
                Message.Invoke("Укажите количество регистров для чтения.", MessageType.Warning);
                return;
            }

            if (Period_ms < Model.Host_ReadTimeout + TimeForReadHandler)
            {
                Message.Invoke("Значение периода опроса не может быть меньше суммы таймаута чтения и " +
                    TimeForReadHandler + " мс. (" + Model.Host_ReadTimeout + " мс. + " + TimeForReadHandler + "мс.)\n" +
                    "Таймаут чтения: " + Model.Host_ReadTimeout + " мс.", MessageType.Warning);

                return;
            }

            ModbusReadFunction ReadFunction = Function.AllReadFunctions.Single(x => x.DisplayedName == SelectedReadFunction);

            Model.Modbus.CycleMode_Period = Period_ms;
            Model.Modbus.CycleMode_Start(async () =>
            {
                await Modbus_Read(_selectedSlaveID, _selectedAddress, ReadFunction, NumberOfRegisters, _checkSum_IsEnable);
            });
        }
    }
}
