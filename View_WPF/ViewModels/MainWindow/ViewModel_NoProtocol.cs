using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Core.Models;
using ReactiveUI;

namespace View_WPF.ViewModels.MainWindow
{
    internal enum SendMessageType
    {
        String,
        Char
    }

    internal class ViewModel_NoProtocol : ReactiveObject
    {
        #region Properties

        private string tx = string.Empty;

        public string TX_String
        {
            get
            {
                return tx;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref tx, value);

                if (Model.HostIsConnect &&
                    tx != string.Empty &&
                    TypeOfSendMessage == SendMessageType.Char)
                {
                    Model.NoProtocol.Send(tx.Last().ToString(), CR_Enable, LF_Enable);
                }
            }
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

        #endregion

        public ReactiveCommand<Unit, Unit> Command_Select_Char { get; }
        public ReactiveCommand<Unit, Unit> Command_Select_String { get; }

        public ReactiveCommand<Unit, Unit> Command_Send { get; }

        public ReactiveCommand<Unit, Unit> Command_ClearRX { get; }

        private SendMessageType TypeOfSendMessage;

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;
        private readonly Action SetUI_Connected;
        private readonly Action SetUI_Disconnected;
        private readonly Action<string> UI_Action_Receive;

        public ViewModel_NoProtocol(
            Action<string, MessageType> MessageBox,
            Action UI_Connected_Handler,
            Action UI_Disconnected_Handler,
            Action<string> Action_Receive_Handler,
            Action Clear_ReceiveField)
        {
            Message = MessageBox;

            SetUI_Connected = UI_Connected_Handler;
            SetUI_Disconnected = UI_Disconnected_Handler;

            UI_Action_Receive = Action_Receive_Handler;

            Model = ConnectedHost.Model;

            SetUI_Disconnected.Invoke();

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Model.NoProtocol.NoProtocol_DataReceived += NoProtocol_NoProtocol_DataReceived;

            Command_Select_Char = ReactiveCommand.Create(Select_Char);
            Command_Select_String = ReactiveCommand.Create(Select_String);

            Command_Send = ReactiveCommand.Create(SendMessage);

            Command_Send.ThrownExceptions.Subscribe(error => Message?.Invoke(error.Message, MessageType.Error));

            Command_ClearRX = ReactiveCommand.Create(Clear_ReceiveField);
        }

        private void NoProtocol_NoProtocol_DataReceived(object? sender, string e)
        {
            UI_Action_Receive.Invoke(e);
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            SetUI_Connected?.Invoke();
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            TX_String = string.Empty;
            SetUI_Disconnected?.Invoke();
        }

        public void Select_Char()
        {
            TypeOfSendMessage = SendMessageType.Char;
            TX_String = string.Empty;
        }

        public void Select_String()
        {
            TypeOfSendMessage = SendMessageType.String;
            TX_String = string.Empty;
        }

        public void SendMessage()
        {
            Model.NoProtocol.Send(TX_String, CR_Enable, LF_Enable);
        }
    }
}
