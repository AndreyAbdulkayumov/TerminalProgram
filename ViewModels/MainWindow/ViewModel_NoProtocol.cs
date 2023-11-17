using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Clients;
using Core.Models;
using ReactiveUI;
using MessageBox_Core;

namespace ViewModels.MainWindow
{
    internal enum SendMessageType
    {
        String,
        Char
    }

    public class ViewModel_NoProtocol : ReactiveObject
    {
        #region Properties

        private const string InterfaceType_Default = "не определен";
        private const string InterfaceType_SerialPort = "Serial Port";
        private const string InterfaceType_Ethernet = "Ethernet";

        private string _interfaceType = InterfaceType_Default;

        public string InterfaceType
        {
            get => _interfaceType;
            set => this.RaiseAndSetIfChanged(ref _interfaceType, value);
        }

        private string _tx_String = String.Empty;

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

        private bool _rx_nextLine;

        public bool RX_NextLine
        {
            get => _rx_nextLine;
            set => this.RaiseAndSetIfChanged(ref _rx_nextLine, value);
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
            Action Clear_ReceiveField
            )
        {
            Message = MessageBox;

            SetUI_Connected = UI_Connected_Handler;
            SetUI_Disconnected = UI_Disconnected_Handler;

            UI_Action_Receive = Action_Receive_Handler;

            Model = ConnectedHost.Model;

            SetUI_Disconnected.Invoke();

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Model.NoProtocol.Model_DataReceived += NoProtocol_Model_DataReceived;
            Model.NoProtocol.Model_ErrorInReadThread += NoProtocol_Model_ErrorInReadThread;

            this.WhenAnyValue(x => x.TX_String)
                .WhereNotNull()
                .Where(x => x != String.Empty)
                .Subscribe(_ =>
                {
                    try
                    {
                        if (Model.HostIsConnect &&
                            TX_String != String.Empty &&
                            TypeOfSendMessage == SendMessageType.Char)
                        {
                            Model.NoProtocol.Send(TX_String.Last().ToString(), CR_Enable, LF_Enable);
                        }
                    }
                    
                    catch (Exception error)
                    {
                        Message.Invoke("Ошибка отправки данных\n\n" + error.Message, MessageType.Error);
                    }
                });

            Command_Select_Char = ReactiveCommand.Create(() =>
            {
                TypeOfSendMessage = SendMessageType.Char;
                TX_String = String.Empty;
            });

            Command_Select_String = ReactiveCommand.Create(() =>
            {
                TypeOfSendMessage = SendMessageType.String;
                TX_String = String.Empty;
            });

            Command_Send = ReactiveCommand.Create(() =>
            {
                Model.NoProtocol.Send(TX_String, CR_Enable, LF_Enable);
            });

            Command_Send.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка отправки данных\n\n" + error.Message, MessageType.Error));

            Command_ClearRX = ReactiveCommand.Create(Clear_ReceiveField);
        }        

        private void Model_DeviceIsConnect(object? sender, ConnectArgs e)
        {
            if (e.ConnectedDevice is IPClient)
            {
                InterfaceType = InterfaceType_Ethernet;
            }

            else if (e.ConnectedDevice is SerialPortClient)
            {
                InterfaceType = InterfaceType_SerialPort;
            }

            else
            {
                Message.Invoke("Задан неизвестный тип подключения.", MessageType.Error);
                return;
            }

            SetUI_Connected?.Invoke();
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            InterfaceType = InterfaceType_Default;

            TX_String = String.Empty;

            SetUI_Disconnected?.Invoke();
        }

        private void NoProtocol_Model_DataReceived(object? sender, string e)
        {
            if (RX_NextLine)
            {
                e += "\n";
            }

            UI_Action_Receive.Invoke(e);
        }

        private void NoProtocol_Model_ErrorInReadThread(object? sender, string e)
        {
            Message.Invoke(e, MessageType.Error);
        }
    }
}
