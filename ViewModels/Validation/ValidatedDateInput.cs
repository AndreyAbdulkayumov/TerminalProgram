using System.Globalization;

namespace ViewModels.Validation
{
    public abstract class ValidatedDateInput : ValidatedDateInputBase
    {
        protected abstract ValidateMessage? GetErrorMessage(string fieldName, string? value);

        protected const string HexError_Byte = "HexError_Byte";
        protected const string HexError_UInt16 = "HexError_UInt16";
        protected const string DecError_Byte = "DecError_Byte";
        protected const string DecError_UInt16 = "DecError_UInt16";
        protected const string DecError_uint = "DecError_uint";
        protected const string IP_Address_Invalid = "IP-Address Invalid";

        protected Dictionary<string, ValidateMessage> AllErrorMessages = new Dictionary<string, ValidateMessage>()
        {
            { HexError_Byte,
                new ValidateMessage(
                    shortMessage: "0x00 - 0xFF",
                    fullMessage:"Допустим ввод только чисел в шестнадцатеричной системе счисления. Диапазон чисел от 0x00 до 0xFF."
                    )},

            { HexError_UInt16,
                new ValidateMessage(
                    shortMessage: "0x0000 - 0xFFFF",
                    fullMessage: "Допустим ввод только чисел в шестнадцатеричной системе счисления. Диапазон чисел от 0x0000 до 0xFFFF."
                    )},

            { DecError_Byte,
                new ValidateMessage(
                    shortMessage: "0 - 255",
                    fullMessage: "Допустим ввод только чисел в десятичной системе счисления. Диапазон чисел от 0 до 255."
                    )},

            { DecError_UInt16,
                new ValidateMessage(
                    shortMessage: "0 - 65535",
                    fullMessage: "Допустим ввод только чисел в десятичной системе счисления. Диапазон чисел от 0 до 65535."
                    )},

            { DecError_uint,
                new ValidateMessage(
                    shortMessage: "0 - 2^32",
                    fullMessage: "Допустим ввод только чисел в десятичной системе счисления. Диапазон чисел от 0 до 2^32."
                    )},

            { IP_Address_Invalid, 
                new ValidateMessage(
                    shortMessage: "Некорректный IP-адрес", 
                    fullMessage: "Некорректный IP-адрес"
                    )},
        };

        public Dictionary<string, ValidateMessage> ActualErrors => _errors;

        protected void ValidateInput(string fieldName, string? value)
        {
            _errors.Remove(fieldName);

            ValidateMessage? message = GetErrorMessage(fieldName, value);

            if (message != null)
            {
                _errors[fieldName] = message;
            }

            OnErrorsChanged(fieldName);
        }

        protected void ChangeNumberStyleInErrors(string fieldName, NumberStyles style)
        {
            switch (style)
            {
                case NumberStyles.Number:
                    ChangeErrorsToDec(fieldName);
                    break;

                case NumberStyles.HexNumber:
                    ChangeErrorsToHex(fieldName);
                    break;
            }
        }

        private void ChangeErrorsToDec(string fieldName)
        {
            if (_errors.ContainsKey(fieldName))
            {
                if (_errors[fieldName].Equals(AllErrorMessages[HexError_Byte]))
                {
                    _errors[fieldName] = AllErrorMessages[DecError_Byte];
                }

                else if (_errors[fieldName].Equals(AllErrorMessages[HexError_UInt16]))
                {
                    _errors[fieldName] = AllErrorMessages[DecError_UInt16];
                }

                OnErrorsChanged(fieldName);
            }            
        }

        private void ChangeErrorsToHex(string fieldName)
        {
            if (_errors.ContainsKey(fieldName))
            {
                if (_errors[fieldName].Equals(AllErrorMessages[DecError_Byte]))
                {
                    _errors[fieldName] = AllErrorMessages[HexError_Byte];
                }

                else if (_errors[fieldName].Equals(AllErrorMessages[DecError_UInt16]))
                {
                    _errors[fieldName] = AllErrorMessages[HexError_UInt16];
                }

                OnErrorsChanged(fieldName);
            }            
        }

        public string? GetFullErrorMessage(string propertyName)
        {
            if (_errors.TryGetValue(propertyName, out var validationMessage))
            {
                return validationMessage.Full;
            }

            return null;
        }
    }
}
