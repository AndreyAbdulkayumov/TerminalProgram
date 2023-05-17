using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using ReactiveUI;

namespace View_WPF.ViewModels
{
    public enum SendMessageType
    {
        String,
        Char
    }

    public class ViewModel_NoProtocol : ReactiveObject
    {
        #region Properties

        private string tx = String.Empty;

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
                    tx != String.Empty &&
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

        private string rx = String.Empty;

        public string RX_String
        {
            get => rx;
            set => this.RaiseAndSetIfChanged(ref rx, value);
        }

        #endregion

        public ReactiveCommand<Unit, Unit> Command_Select_Char { get; }
        public ReactiveCommand<Unit, Unit> Command_Select_String { get; }

        public ReactiveCommand<Unit, Unit> Command_Send { get; }

        private SendMessageType TypeOfSendMessage;

        private readonly ConnectedHost Model;

        private readonly ViewMessage Message;
        private readonly StateUI_Connected SetUI_Connected;
        private readonly StateUI_Disconnected SetUI_Disconnected;
        private readonly ActionAfter_Receive UIAction_Receive;

        public ViewModel_NoProtocol(
            ViewMessage MessageBox,
            StateUI_Connected UI_Connected_Handler,
            StateUI_Disconnected UI_Disconnected_Handler,
            ActionAfter_Receive ActionAfter_Receive_Handler)
        {
            Message = MessageBox;

            SetUI_Connected = UI_Connected_Handler;
            SetUI_Disconnected = UI_Disconnected_Handler;

            UIAction_Receive = ActionAfter_Receive_Handler;

            Model = ConnectedHost.Model;

            SetUI_Disconnected.Invoke();

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;
            

            Command_Select_Char = ReactiveCommand.Create(Select_Char);
            Command_Select_String = ReactiveCommand.Create(Select_String);

            Command_Send = ReactiveCommand.Create(SendMessage);

            Command_Send.ThrownExceptions.Subscribe(error => Message?.Invoke(error.Message, MessageType.Error));
        }

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            SetUI_Connected?.Invoke();
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            TX_String = String.Empty;
            SetUI_Disconnected?.Invoke();
        }

        public void Select_Char()
        {
            TypeOfSendMessage = SendMessageType.Char;
            TX_String = String.Empty;
        }

        public void Select_String()
        {
            TypeOfSendMessage = SendMessageType.String;
            TX_String = String.Empty;
        }

        public void SendMessage()
        {
            Model.NoProtocol.Send(TX_String, CR_Enable, LF_Enable);
        }
    }
}
