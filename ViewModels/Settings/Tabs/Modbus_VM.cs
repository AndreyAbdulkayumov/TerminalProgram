using ReactiveUI;
using System.Globalization;
using System.Reactive.Linq;
using ViewModels.Validation;

namespace ViewModels.Settings.Tabs
{
    public class Modbus_VM : ValidatedDateInput
    {
        private string _writeTimeout = string.Empty;

        public string WriteTimeout
        {
            get => _writeTimeout;
            set
            {
                this.RaiseAndSetIfChanged(ref _writeTimeout, value);
                ValidateInput(nameof(WriteTimeout), value);
            }
        }

        private string _readTimeout = string.Empty;

        public string ReadTimeout
        {
            get => _readTimeout;
            set 
            {
                this.RaiseAndSetIfChanged(ref _readTimeout, value);
                ValidateInput(nameof(ReadTimeout), value);
            }
        }

        public Modbus_VM()
        {

        }

        protected override IEnumerable<string> GetShortErrorMessages(string fieldName, string? value)
        {
            List<ValidateMessage> errors = new List<ValidateMessage>();

            if (string.IsNullOrEmpty(value))
            {
                return errors.Select(message => message.Short);
            }

            if (!StringValue.IsValidNumber(value, NumberStyles.Number, out uint _))
            {
                errors.Add(AllErrorMessages[DecError_uint]);
            }

            return errors.Select(message => message.Short);
        }
    }
}
