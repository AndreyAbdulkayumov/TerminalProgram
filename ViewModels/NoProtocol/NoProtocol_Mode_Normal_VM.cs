using Core.Models;
using MessageBox_Core;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace ViewModels.NoProtocol
{
    internal enum SendMessageType
    {
        String,
        Char
    }

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

        private bool _messageIsChar;

        public bool MessageIsChar
        {
            get => _messageIsChar;
            set => this.RaiseAndSetIfChanged(ref _messageIsChar, value);
        }

        private bool _messageIsString;

        public bool MessageIsString
        {
            get => _messageIsString;
            set => this.RaiseAndSetIfChanged(ref _messageIsString, value);
        }

        public ReactiveCommand<Unit, Unit> Command_Select_Char { get; }
        public ReactiveCommand<Unit, Unit> Command_Select_String { get; }
        public ReactiveCommand<Unit, Unit> Command_Send { get; }
                
        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;

        private SendMessageType _typeOfSendMessage;


        public NoProtocol_Mode_Normal_VM(Action<string, MessageType> messageBox)
        {
            Message = messageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            this.WhenAnyValue(x => x.TX_String)
                .WhereNotNull()
                .Where(x => x != string.Empty)
                .Subscribe(async _ =>
                {
                    try
                    {
                        if (Model.HostIsConnect &&
                            TX_String != string.Empty &&
                            _typeOfSendMessage == SendMessageType.Char)
                        {
                            await Model.NoProtocol.Send(TX_String.Last().ToString(), CR_Enable, LF_Enable);
                        }
                    }

                    catch (Exception error)
                    {
                        Message.Invoke("Ошибка отправки данных\n\n" + error.Message, MessageType.Error);
                    }
                });

            Command_Select_Char = ReactiveCommand.Create(() =>
            {
                _typeOfSendMessage = SendMessageType.Char;
                TX_String = string.Empty;
            });

            Command_Select_String = ReactiveCommand.Create(() =>
            {
                _typeOfSendMessage = SendMessageType.String;
                TX_String = string.Empty;
            });

            Command_Send = ReactiveCommand.CreateFromTask(async () =>
            {
                await Model.NoProtocol.Send(TX_String, CR_Enable, LF_Enable);
            });

            Command_Send.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка отправки данных.\n\n" + error.Message, MessageType.Error));
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
