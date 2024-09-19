namespace ViewModels.Validation
{
    public abstract class ValidatedDateInput : ValidatedDateInputBase
    {
        protected abstract IEnumerable<string> GetShortErrorMessages(string fieldName, string? value);

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
                    fullMessage:"Допустим ввод только чисел в шестнадцатеричной системе счисления.\\n\\nДиапазон чисел от 0x00 до 0xFF."
                    )},

            { HexError_UInt16,
                new ValidateMessage(
                    shortMessage: "0x0000 - 0xFFFF",
                    fullMessage: "Допустим ввод только чисел в шестнадцатеричной системе счисления.\n\nДиапазон чисел от 0x0000 до 0xFFFF."
                    )},

            { DecError_Byte,
                new ValidateMessage(
                    shortMessage: "0 - 255",
                    fullMessage: "Допустим ввод только чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 255."
                    )},

            { DecError_UInt16,
                new ValidateMessage(
                    shortMessage: "0 - 65535",
                    fullMessage: "Допустим ввод только чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 65535."
                    )},

            { DecError_uint,
                new ValidateMessage(
                    shortMessage: "0 - 2^32",
                    fullMessage: "Допустим ввод только чисел в десятичной системе счисления.\n\nДиапазон чисел от 0 до 2^32."
                    )},

            { IP_Address_Invalid, 
                new ValidateMessage(
                    shortMessage: "Некорректный IP-адрес", 
                    fullMessage: "Некорректный IP-адрес"
                    )},
        };

        public Dictionary<string, List<string>> ActualErrors => _errors;

        protected void ValidateInput(string fieldName, string? value)
        {
            _errors.Remove(fieldName);

            IEnumerable<string> messages = GetShortErrorMessages(fieldName, value);

            if (messages.Count() > 0)
            {
                _errors[fieldName] = [.. messages];
            }

            OnErrorsChanged(fieldName);
        }

        public string? GetErrorsMessage(string propertyName)
        {
            var messages = (IEnumerable<string>)GetErrors(propertyName);

            return string.Join(" ", messages);
        }
    }
}
