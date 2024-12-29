using Core.Models;
using Core.Models.Settings.FileTypes;
using ReactiveUI;
using ViewModels.Helpers;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros.MacrosEdit
{
    public class NoProtocolMacros_VM : ReactiveObject, IMacrosContent<MacrosNoProtocolItem>
    {
        private string _messageString = string.Empty;

        public string MessageString
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
                MessageString = data.Message;
                IsBytesSend = data.IsByteString;
                CR_Enable = data.EnableCR;
                LF_Enable = data.EnableLF;
            }

            this.WhenAnyValue(x => x.IsBytesSend)
                .Subscribe(IsBytes =>
                {
                    if (_isInit)
                    {
                        MessageString = StringByteConverter.GetMessageString(MessageString, IsBytes, ConnectedHost.Model.NoProtocol.HostEncoding);
                    }
                    
                });

            _isInit = true;
        }

        public MacrosNoProtocolItem GetContent()
        {
            return new MacrosNoProtocolItem()
            {
                Message = MessageString,
                IsByteString = IsBytesSend,
                EnableCR = CR_Enable,
                EnableLF = LF_Enable,
            };
        }

        public string GetValidatedString()
        {
            if (IsBytesSend)
            {
                return StringByteConverter.GetValidatedByteString(MessageString);
            }

            return MessageString;
        }
    }
}
