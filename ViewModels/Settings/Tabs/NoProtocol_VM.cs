using ReactiveUI;
using System.Collections.ObjectModel;
using System.Globalization;
using ViewModels.Validation;

namespace ViewModels.Settings.Tabs
{
    public class NoProtocol_VM : ValidatedDateInput, IValidationFieldInfo
    {
        private readonly ObservableCollection<string> _typeOfEncoding = new ObservableCollection<string>()
        {
            "ASCII", "UTF-8", "UTF-32", "Unicode"
        };

        public ObservableCollection<string> TypeOfEncoding
        {
            get => _typeOfEncoding;
        }

        private string _selectedEncoding = string.Empty;

        public string SelectedEncoding
        {
            get => _selectedEncoding;
            set => this.RaiseAndSetIfChanged(ref _selectedEncoding, value);
        }

        private string _selectedReceiveBufferSize = string.Empty;

        public string SelectedReceiveBufferSize
        {
            get => _selectedReceiveBufferSize;
            set 
            {
                this.RaiseAndSetIfChanged(ref _selectedReceiveBufferSize, value);
                ValidateInput(nameof(SelectedReceiveBufferSize), value);
            }
        }

        public NoProtocol_VM()
        {

        }

        public string GetFieldViewName(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(SelectedReceiveBufferSize):
                    return "Размер буфера приема";

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

            if (!StringValue.IsValidNumber(value, NumberStyles.Number, out uint _))
            {
                return AllErrorMessages[DecError_uint];
            }

            return null;
        }
    }
}
