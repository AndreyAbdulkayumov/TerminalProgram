using Core.Models.Modbus;
using Core.Models.Settings.FileTypes;
using ReactiveUI;
using System.Collections.ObjectModel;
using ViewModels.ModbusClient.WriteFields;
using ViewModels.Validation;

namespace ViewModels.Macros.MacrosEdit
{
    public class ModbusMacros_VM : ValidatedDateInput, IValidationFieldInfo, IMacrosContent<MacrosModbusItem>
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

        private readonly IWriteField_VM WriteField_MultipleCoils_VM;
        private readonly IWriteField_VM WriteField_MultipleRegisters_VM;
        private readonly IWriteField_VM WriteField_SingleCoil_VM;
        private readonly IWriteField_VM WriteField_SingleRegister_VM;

        public ModbusMacros_VM()
        {
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

        public MacrosModbusItem GetContent()
        {
            return new MacrosModbusItem();
        }

        public string GetFieldViewName(string fieldName)
        {
            throw new NotImplementedException();
        }

        protected override ValidateMessage? GetErrorMessage(string fieldName, string? value)
        {
            throw new NotImplementedException();
        }
    }
}
