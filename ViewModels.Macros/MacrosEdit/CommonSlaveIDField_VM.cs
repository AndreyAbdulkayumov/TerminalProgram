using MessageBox_Core;
using ReactiveUI;
using System.Globalization;
using ViewModels.Validation;

namespace ViewModels.Macros.MacrosEdit
{
    public class CommonSlaveIDField_VM : ValidatedDateInput
    {
        public event EventHandler<bool>? UseCommonSlaveIdChanged;

        private bool _useCommonSlaveId;

        public bool UseCommonSlaveId
        {
            get => _useCommonSlaveId;
            set => this.RaiseAndSetIfChanged(ref _useCommonSlaveId, value);
        }

        private string? _commonSlaveId;

        public string? CommonSlaveId
        {
            get => _commonSlaveId;
            set
            {
                this.RaiseAndSetIfChanged(ref _commonSlaveId, value);
                ValidateInput(nameof(CommonSlaveId), value);
            }
        }

        private bool _numberFormat_Hex;

        public bool NumberFormat_Hex
        {
            get => _numberFormat_Hex;
            set => this.RaiseAndSetIfChanged(ref _numberFormat_Hex, value);
        }

        private bool _numberFormat_Dec;

        public bool NumberFormat_Dec
        {
            get => _numberFormat_Dec;
            set => this.RaiseAndSetIfChanged(ref _numberFormat_Dec, value);
        }

        private NumberStyles _numberViewStyle;
        private UInt16 _selectedSlaveID;

        public CommonSlaveIDField_VM(IMessageBox messageBox)
        {
            NumberFormat_Hex = true;

            this.WhenAnyValue(x => x.UseCommonSlaveId)
                .Subscribe(x => UseCommonSlaveIdChanged?.Invoke(this, x));

            this.WhenAnyValue(x => x.NumberFormat_Hex, x => x.NumberFormat_Dec)
                .Subscribe(values =>
                {
                    try
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
                    }

                    catch (Exception error)
                    {
                        messageBox.Show($"Ошибка смены формата.\n\n{error.Message}", MessageType.Error, error);
                    }
                });
        }

        private void SelectNumberFormat_Hex()
        {
            _numberViewStyle = NumberStyles.HexNumber;

            if (!string.IsNullOrWhiteSpace(CommonSlaveId) && string.IsNullOrEmpty(GetFullErrorMessage(nameof(CommonSlaveId))))
            {
                CommonSlaveId = _selectedSlaveID.ToString("X");
            }

            else
            {
                _selectedSlaveID = 0;
            }

            ValidateInput(nameof(CommonSlaveId), CommonSlaveId);

            ChangeNumberStyleInErrors(nameof(CommonSlaveId), NumberStyles.HexNumber);
        }

        private void SelectNumberFormat_Dec()
        {
            _numberViewStyle = NumberStyles.Number;

            if (!string.IsNullOrWhiteSpace(CommonSlaveId) && string.IsNullOrEmpty(GetFullErrorMessage(nameof(CommonSlaveId))))
            {
                CommonSlaveId = int.Parse(CommonSlaveId, NumberStyles.HexNumber).ToString();
            }

            else
            {
                _selectedSlaveID = 0;
            }

            ValidateInput(nameof(CommonSlaveId), CommonSlaveId);

            ChangeNumberStyleInErrors(nameof(CommonSlaveId), NumberStyles.Number);
        }

        protected override ValidateMessage? GetErrorMessage(string fieldName, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            switch (fieldName)
            {
                case nameof(CommonSlaveId):
                    return Check_SlaveID(value);
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
    }
}
