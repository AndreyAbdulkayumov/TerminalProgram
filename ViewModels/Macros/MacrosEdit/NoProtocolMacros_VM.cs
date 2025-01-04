using Core.Models.Settings.FileTypes;
using ReactiveUI;
using System.Collections.ObjectModel;
using ViewModels.Helpers;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros.MacrosEdit
{
    public class NoProtocolMacros_VM : ReactiveObject, IMacrosContent<MacrosNoProtocolItem>
    {
        private readonly ObservableCollection<string> _typeOfEncoding = new ObservableCollection<string>()
        {
            AppEncoding.Name_ASCII, AppEncoding.Name_UTF8, AppEncoding.Name_UTF32, AppEncoding.Name_Unicode
        };

        public ObservableCollection<string> TypeOfEncoding
        {
            get => _typeOfEncoding;
        }

        private string? _selectedEncoding = AppEncoding.Name_UTF8;

        public string? SelectedEncoding
        {
            get => _selectedEncoding;
            set => this.RaiseAndSetIfChanged(ref _selectedEncoding, value);
        }

        private string? _messageString;

        public string? MessageString
        {
            get => _messageString;
            set => this.RaiseAndSetIfChanged(ref _messageString, value);
        }

        private bool _cr_enable;

        public bool CR_Enable
        {
            get => _cr_enable;
            set => this.RaiseAndSetIfChanged(ref _cr_enable, value);
        }

        private bool _lf_enable;

        public bool LF_Enable
        {
            get => _lf_enable;
            set => this.RaiseAndSetIfChanged(ref _lf_enable, value);
        }

        private bool _isBytesSend;

        public bool IsBytesSend
        {
            get => _isBytesSend;
            set => this.RaiseAndSetIfChanged(ref _isBytesSend, value);
        }

        private bool _isInit;

        public NoProtocolMacros_VM(object? initData)
        {
            _isInit = false;

            if (initData is MacrosNoProtocolItem data)
            {
                SelectedEncoding = string.IsNullOrEmpty(data.MacrosEncoding) ? AppEncoding.Name_UTF8 : data.MacrosEncoding;
                MessageString = data.Message;
                IsBytesSend = data.IsByteString;
                CR_Enable = data.EnableCR;
                LF_Enable = data.EnableLF;
            }

            this.WhenAnyValue(x => x.IsBytesSend)
                .Subscribe(IsBytes =>
                {
                    if (_isInit && !string.IsNullOrEmpty(MessageString))
                    {
                        MessageString = StringByteConverter.GetMessageString(MessageString, IsBytes, AppEncoding.GetEncoding(SelectedEncoding));
                    }
                    
                });

            _isInit = true;
        }

        public MacrosNoProtocolItem GetContent()
        {
            return new MacrosNoProtocolItem()
            {
                MacrosEncoding = SelectedEncoding,
                Message = MessageString,
                IsByteString = IsBytesSend,
                EnableCR = CR_Enable,
                EnableLF = LF_Enable,
            };
        }

        public string GetValidatedString()
        {
            if (string.IsNullOrEmpty(MessageString))
            {
                return string.Empty;
            }

            if (IsBytesSend)
            {
                return StringByteConverter.GetValidatedByteString(MessageString);
            }

            return MessageString;
        }
    }
}
