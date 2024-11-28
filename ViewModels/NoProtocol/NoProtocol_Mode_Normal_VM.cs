using Core.Models;
using MessageBox_Core;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

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


        public NoProtocol_Mode_Normal_VM(IMessageBox messageBox)
        {
            _messageBox = messageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Command_Send = ReactiveCommand.CreateFromTask(async () =>
            {
                if (IsBytesSend)
                {
                    await Model.NoProtocol.SendBytes(ConvertToBytes(TX_String));
                    return;
                }

                await Model.NoProtocol.SendString(TX_String, CR_Enable, LF_Enable);
            });

            Command_Send.ThrownExceptions.Subscribe(error => _messageBox.Show("Ошибка отправки данных.\n\n" + error.Message, MessageType.Error));
        }

        public string GetValidatedString()
        {
            if (IsBytesSend)
            {
                return Regex.Replace(TX_String, @"[^0-9a-fA-F\s]", string.Empty).ToUpper();
            }

            return TX_String; 
        }

        private byte[] ConvertToBytes(string message)
        {
            message = message.Replace(" ", string.Empty);

            byte[] bytesToSend = new byte[message.Length / 2 + message.Length % 2];

            string byteString;

            for (int i = 0; i < bytesToSend.Length; i++)
            {
                if (i * 2 + 2 > message.Length)
                    byteString = "0" + message.Last();
                else
                    byteString = message.Substring(i * 2, 2);

                bytesToSend[i] = Convert.ToByte(byteString, 16);
            }

            return bytesToSend;
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            UI_IsEnable = true;
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            UI_IsEnable = false;
        }
    }
}
