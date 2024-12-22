using Core.Models.Modbus;
using Core.Models.Settings.FileTypes;
using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using ViewModels.ModbusClient;
using ViewModels.ModbusClient.WriteFields;
using ViewModels.Validation;

namespace ViewModels.Macros.MacrosEdit
{
    public class ModbusMacros_VM : ValidatedDateInput, IValidationFieldInfo, IMacrosContent<MacrosModbusItem>, IMacrosValidation
    {
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

        private bool _checkSum_IsEnable;

        public bool CheckSum_IsEnable
        {
            get => _checkSum_IsEnable;
            set => this.RaiseAndSetIfChanged(ref _checkSum_IsEnable, value);
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

        private bool _selectedFunctionType_Read;

        public bool SelectedFunctionType_Read
        {
            get => _selectedFunctionType_Read;
            set => this.RaiseAndSetIfChanged(ref _selectedFunctionType_Read, value);
        }

        private bool _selectedFunctionType_Write;

        public bool SelectedFunctionType_Write
        {
            get => _selectedFunctionType_Write;
            set => this.RaiseAndSetIfChanged(ref _selectedFunctionType_Write, value);
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

        private NumberStyles _numberViewStyle;

        private byte _selectedSlaveID = 0;
        private ushort _selectedAddress = 0;
        private ushort _selectedNumberOfRegisters = 1;

        private readonly IWriteField_VM WriteField_MultipleCoils_VM;
        private readonly IWriteField_VM WriteField_MultipleRegisters_VM;
        private readonly IWriteField_VM WriteField_SingleCoil_VM;
        private readonly IWriteField_VM WriteField_SingleRegister_VM;

        private readonly IMessageBox _messageBox;

        public ModbusMacros_VM(IMessageBox messageBox)
        {
            _messageBox = messageBox;

            WriteField_MultipleCoils_VM = new MultipleCoils_VM();
            WriteField_MultipleRegisters_VM = new MultipleRegisters_VM();
            WriteField_SingleCoil_VM = new SingleCoil_VM();
            WriteField_SingleRegister_VM = new SingleRegister_VM();

            /****************************************************/
            //
            // Первоначальная настройка UI
            //
            /****************************************************/

            CheckSum_IsEnable = true;

            SelectedNumberFormat_Hex = true;

            SelectedFunctionType_Read = true;

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

        public MacrosModbusItem GetContent()
        {
            var selectedFunction = SelectedFunctionType_Read ? SelectedReadFunction : SelectedWriteFunction;

            int functionNumber = Function.AllFunctions.Single(x => x.DisplayedName == selectedFunction).Number;

            return new MacrosModbusItem()
            {
                SlaveID = _selectedSlaveID,
                Address = _selectedAddress,
                FunctionNumber = functionNumber,
                WriteBuffer = SelectedFunctionType_Write ? CurrentWriteFieldViewModel?.GetData().Data : null,
                NumberOfRegisters = _selectedNumberOfRegisters,
                CheckSum_IsEnable = CheckSum_IsEnable,
            };
        }

        public string? GetValidationMessage()
        {
            return SelectedFunctionType_Write ? CheckWriteFields() : CheckReadFields();
        }

        private string? CheckWriteFields()
        {
            StringBuilder message = new StringBuilder();

            // Проверка полей в основном контроле

            if (HasErrors)
            {
                foreach (KeyValuePair<string, ValidateMessage> element in ActualErrors)
                {
                    if (element.Key == nameof(NumberOfRegisters))
                    {
                        continue;
                    }

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

        protected override ValidateMessage? GetErrorMessage(string fieldName, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            switch (fieldName)
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
