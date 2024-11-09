using Core.Clients;
using Core.Models;
using Core.Models.Modbus;
using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using ViewModels.ModbusClient.WriteFields;
using ViewModels.Validation;

namespace ViewModels.ModbusClient
{
    public class ModbusClient_Mode_Normal_VM : ValidatedDateInput, IValidationFieldInfo
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
            set 
            {
                this.RaiseAndSetIfChanged(ref _slaveID, value);
                ValidateInput(nameof(SlaveID), value);
            }
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
            set 
            {
                this.RaiseAndSetIfChanged(ref _address, value);
                ValidateInput(nameof(Address), value);
            } 
        }

        private string? _numberOfRegisters;

        public string? NumberOfRegisters
        {
            get => _numberOfRegisters;
            set
            {
                this.RaiseAndSetIfChanged(ref _numberOfRegisters, value);
                ValidateInput(nameof(NumberOfRegisters), value);
            }
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

        private IWriteField_VM? _currentWriteFieldViewModel;

        public IWriteField_VM? CurrentWriteFieldViewModel
        {
            get => _currentWriteFieldViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentWriteFieldViewModel, value);
        }


        public ReactiveCommand<Unit, Unit> Command_Read { get; }
        public ReactiveCommand<Unit, Unit> Command_Write { get; }


        private readonly List<ushort> WriteBuffer = new List<ushort>();

        private NumberStyles _numberViewStyle;

        private byte _selectedSlaveID = 0;
        private ushort _selectedAddress = 0;
        private ushort _selectedNumberOfRegisters = 1;

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;

        private readonly IWriteField_VM WriteField_MultipleCoils_VM;
        private readonly IWriteField_VM WriteField_MultipleRegisters_VM;
        private readonly IWriteField_VM WriteField_SingleCoil_VM;
        private readonly IWriteField_VM WriteField_SingleRegister_VM;


        public ModbusClient_Mode_Normal_VM(
            Action<string, MessageType> messageBox,
            Func<byte, ushort, ModbusWriteFunction, byte[], int, bool, Task> modbus_Write,
            Func<byte, ushort, ModbusReadFunction, int, bool, Task> modbus_Read
            )
        {
            Message = messageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            WriteField_MultipleCoils_VM = new MultipleCoils_VM();
            WriteField_MultipleRegisters_VM = new MultipleRegisters_VM();
            WriteField_SingleCoil_VM = new SingleCoil_VM();
            WriteField_SingleRegister_VM = new SingleRegister_VM(); 

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
                if (string.IsNullOrEmpty(SlaveID))
                {
                    Message.Invoke("Укажите Slave ID.", MessageType.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(Address))
                {
                    Message.Invoke("Укажите адрес Modbus регистра.", MessageType.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(NumberOfRegisters))
                {
                    Message.Invoke("Укажите количество регистров для чтения.", MessageType.Warning);
                    return;
                }

                string? validationMessage = CheckReadFields();

                if (!string.IsNullOrEmpty(validationMessage))
                {
                    Message.Invoke(validationMessage, MessageType.Warning);
                    return;
                }

                ModbusReadFunction ReadFunction = Function.AllReadFunctions.Single(x => x.DisplayedName == SelectedReadFunction);

                await modbus_Read(_selectedSlaveID, _selectedAddress, ReadFunction, _selectedNumberOfRegisters, CheckSum_Enable);
            });
            Command_Read.ThrownExceptions.Subscribe(error => Message.Invoke("Возникла ошибка при попытке чтения: \n\n" + error.Message, MessageType.Error));

            Command_Write = ReactiveCommand.CreateFromTask(async () =>
            {
                if (string.IsNullOrEmpty(SlaveID))
                {
                    Message.Invoke("Укажите Slave ID.", MessageType.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(Address))
                {
                    Message.Invoke("Укажите адрес Modbus регистра.", MessageType.Warning);
                    return;
                }

                if (CurrentWriteFieldViewModel == null)
                {
                    Message.Invoke("Не выбран тип поля записи Modbus.", MessageType.Warning);
                    return;
                }

                string? validationMessage = CheckWriteFields();

                if (!string.IsNullOrEmpty(validationMessage))
                {
                    Message.Invoke(validationMessage, MessageType.Warning);
                    return;
                }

                ModbusWriteFunction writeFunction = Function.AllWriteFunctions.Single(x => x.DisplayedName == SelectedWriteFunction);

                WriteData modbusWriteData = CurrentWriteFieldViewModel.GetData();

                if (modbusWriteData.Data.Length == 0)
                {
                    Message.Invoke("Укажите данные для записи.", MessageType.Warning);
                    return;
                }

                await modbus_Write(_selectedSlaveID, _selectedAddress, writeFunction, modbusWriteData.Data, modbusWriteData.NumberOfRegisters, CheckSum_Enable);
            });
            Command_Write.ThrownExceptions.Subscribe(error => Message.Invoke("Возникла ошибка при попытке записи:\n\n" + error.Message, MessageType.Error));

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

            this.WhenAnyValue(x => x.SelectedWriteFunction)
                .WhereNotNull()
                .Subscribe(x =>
                {
                    if (x == Function.ForceMultipleCoils.DisplayedName)
                    {
                        CurrentWriteFieldViewModel = WriteField_MultipleCoils_VM;
                    }

                    else if (x == Function.PresetMultipleRegisters.DisplayedName)
                    {
                        CurrentWriteFieldViewModel = WriteField_MultipleRegisters_VM;
                    }

                    else if (x == Function.ForceSingleCoil.DisplayedName)
                    {
                        CurrentWriteFieldViewModel = WriteField_SingleCoil_VM;
                    }

                    else if (x == Function.PresetSingleRegister.DisplayedName)
                    {
                        CurrentWriteFieldViewModel = WriteField_SingleRegister_VM;
                    }
                });
        }

        public string GetFieldViewName(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(SlaveID):
                    return "Slave ID";

                case nameof(Address):
                    return "Адрес";

                case nameof(NumberOfRegisters):
                    return "Кол-во регистров";

                default:
                    return fieldName;
            }
        }

        private string? CheckWriteFields()
        {
            StringBuilder message = new StringBuilder();

            // Проверка полей в основном контроле

            if (HasErrors)
            {
                foreach (KeyValuePair<string, ValidateMessage> element in ActualErrors)
                {
                    message.AppendLine($"[{GetFieldViewName(element.Key)}]\n{GetFullErrorMessage(element.Key)}\n");
                }
            }

            // Проверка полей в текущем контроле записи

            if (CurrentWriteFieldViewModel != null && CurrentWriteFieldViewModel.HasValidationErrors)
            {
                message.AppendLine(CurrentWriteFieldViewModel.ValidationMessage);
            }

            if (message.Length > 0)
            {
                message.Insert(0, "Ошибки валидации\n\n");
                return message.ToString().TrimEnd('\r', '\n');
            }

            return null;
        }

        private string? CheckReadFields()
        {
            if (!HasErrors)
            {
                return null;
            }
            
            StringBuilder message = new StringBuilder();

            // Проверка полей в основном контроле

            foreach (KeyValuePair<string, ValidateMessage> element in ActualErrors)
            {
                message.AppendLine($"[{GetFieldViewName(element.Key)}]\n{GetFullErrorMessage(element.Key)}\n");
            }

            if (message.Length > 0)
            {
                message.Insert(0, "Ошибки валидации\n\n");
                return message.ToString().TrimEnd('\r', '\n');
            }

            return null;
        }

        private void SelectNumberFormat_Hex()
        {
            NumberFormat = ModbusClient_VM.ViewContent_NumberStyle_hex;
            _numberViewStyle = NumberStyles.HexNumber;            

            if (SlaveID != null && string.IsNullOrEmpty(GetFullErrorMessage(nameof(SlaveID))))
            {
                SlaveID = _selectedSlaveID.ToString("X");
            }

            if (Address != null && string.IsNullOrEmpty(GetFullErrorMessage(nameof(Address))))
            {
                Address = _selectedAddress.ToString("X");
            }

            ValidateInput(nameof(SlaveID), SlaveID);
            ValidateInput(nameof(Address), Address);

            ChangeNumberStyleInErrors(nameof(SlaveID), NumberStyles.HexNumber);
            ChangeNumberStyleInErrors(nameof(Address), NumberStyles.HexNumber);
        }

        private void SelectNumberFormat_Dec()
        {
            NumberFormat = ModbusClient_VM.ViewContent_NumberStyle_dec;
            _numberViewStyle = NumberStyles.Number;

            if (SlaveID != null && string.IsNullOrEmpty(GetFullErrorMessage(nameof(SlaveID))))
            {
                SlaveID = int.Parse(SlaveID, NumberStyles.HexNumber).ToString();
            }

            if (Address != null && string.IsNullOrEmpty(GetFullErrorMessage(nameof(Address))))
            {
                Address = int.Parse(Address, NumberStyles.HexNumber).ToString();
            }

            ValidateInput(nameof(SlaveID), SlaveID);
            ValidateInput(nameof(Address), Address);

            ChangeNumberStyleInErrors(nameof(SlaveID), NumberStyles.Number);
            ChangeNumberStyleInErrors(nameof(Address), NumberStyles.Number);
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

        protected override ValidateMessage? GetErrorMessage(string fieldName, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            switch(fieldName)
            {
                case nameof(SlaveID):
                    return Check_SlaveID(value);

                case nameof(Address):
                    return Check_Address(value);

                case nameof(NumberOfRegisters):
                    return Check_NumberOfRegisters(value);
            }

            return null;
        }

        private ValidateMessage? Check_SlaveID(string value)
        {
            if (!StringValue.IsValidNumber(value, _numberViewStyle, out _selectedSlaveID))
            {
                switch (_numberViewStyle)
                {
                    case NumberStyles.Number:
                        return AllErrorMessages[DecError_Byte];

                    case NumberStyles.HexNumber:
                        return AllErrorMessages[HexError_Byte];
                }
            }

            return null;
        }

        private ValidateMessage? Check_Address(string value)
        {
            if (!StringValue.IsValidNumber(value, _numberViewStyle, out _selectedAddress))
            {
                switch (_numberViewStyle)
                {
                    case NumberStyles.Number:
                        return AllErrorMessages[DecError_UInt16];

                    case NumberStyles.HexNumber:
                        return AllErrorMessages[HexError_UInt16];
                }                
            }

            return null;
        }

        private ValidateMessage? Check_NumberOfRegisters(string value)
        {
            if (!StringValue.IsValidNumber(value, NumberStyles.Number, out _selectedNumberOfRegisters))
            {
                return AllErrorMessages[DecError_UInt16];
            }

            return null;
        }
    }
}
