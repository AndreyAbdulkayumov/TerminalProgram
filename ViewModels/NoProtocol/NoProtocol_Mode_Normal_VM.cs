using Core.Clients.DataTypes;
using Core.Models;
using MessageBox_Core;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using ViewModels.Helpers;

namespace ViewModels.NoProtocol
{
    public class NoProtocol_Mode_Normal_VM : ReactiveObject
    {
        private bool ui_IsEnable = false;

        public bool UI_IsEnable
        {
            get => ui_IsEnable;
            set => this.RaiseAndSetIfChanged(ref ui_IsEnable, value);
        }

        private string _tx_String = string.Empty;

        public string TX_String
        {
            get => _tx_String;
            set => this.RaiseAndSetIfChanged(ref _tx_String, value);
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

        public ReactiveCommand<Unit, Unit> Command_Send { get; }
                
        private readonly ConnectedHost Model;

        private readonly IMessageBox _messageBox;

        public NoProtocol_Mode_Normal_VM(
            IMessageBox messageBox, 
            Func<bool, string, bool, bool, Task> sendAction
            )
        {
            _messageBox = messageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Command_Send = ReactiveCommand.CreateFromTask(async () =>
            {
                await sendAction(IsBytesSend, TX_String, CR_Enable, LF_Enable);
            });
            Command_Send.ThrownExceptions.Subscribe(error => _messageBox.Show("Ошибка отправки данных.\n\n" + error.Message, MessageType.Error));

            this.WhenAnyValue(x => x.IsBytesSend)
                .Subscribe(IsBytes =>
                {
                    TX_String = StringByteConverter.GetMessageString(TX_String, IsBytes, ConnectedHost.Model.NoProtocol.HostEncoding);
                });
        }

        public string GetValidatedString()
        {
            if (IsBytesSend)
            {
                return StringByteConverter.GetValidatedByteString(TX_String);
            }

            return TX_String; 
        }

        private void Model_DeviceIsConnect(object? sender, IConnection? e)
        {
            UI_IsEnable = true;
        }

        private void Model_DeviceIsDisconnected(object? sender, IConnection? e)
        {
            UI_IsEnable = false;
        }
    }
}
