using System.Reactive;
using System.Reactive.Linq;
using System.Text;
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

        private bool ui_IsEnable = false;

        public bool UI_IsEnable
        {
            get => ui_IsEnable;
            set => this.RaiseAndSetIfChanged(ref ui_IsEnable, value);
        }

        private const string InterfaceType_Default = "не определен";
        private const string InterfaceType_SerialPort = "Serial Port";
        private const string InterfaceType_Ethernet = "Ethernet";

        private string _interfaceType = InterfaceType_Default;

        public string InterfaceType
        {
            get => _interfaceType;
            set => this.RaiseAndSetIfChanged(ref _interfaceType, value);
        }

        private string _rx_String = String.Empty;

        public string RX_String
        {
            get => _rx_String;
            set => this.RaiseAndSetIfChanged(ref _rx_String, value);
        }

        // Делаем эти значения одинаковыми, чтобы не тратить ресурсы на дополнительное выделение памяти.
        private const int RX_Capacity = 300;
        private const int RX_MaxCapacity = 300;

        private readonly StringBuilder RX = new StringBuilder(RX_Capacity, RX_MaxCapacity);

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

        private bool _rx_NextLine;

        public bool RX_NextLine
        {
            get => _rx_NextLine;
            set => this.RaiseAndSetIfChanged(ref _rx_NextLine, value);
        }

        #endregion

        public ReactiveCommand<Unit, Unit> Command_Select_Char { get; }
        public ReactiveCommand<Unit, Unit> Command_Select_String { get; }

        public ReactiveCommand<Unit, Unit> Command_Send { get; }

        public ReactiveCommand<Unit, Unit> Command_ClearRX { get; }

        private SendMessageType TypeOfSendMessage;

        private readonly ConnectedHost Model;

        private readonly Action<string, MessageType> Message;


        public ViewModel_NoProtocol(Action<string, MessageType> MessageBox)
        {
            Message = MessageBox;

            Model = ConnectedHost.Model;

            Model.DeviceIsConnect += Model_DeviceIsConnect;
            Model.DeviceIsDisconnected += Model_DeviceIsDisconnected;

            Model.NoProtocol.Model_DataReceived += NoProtocol_Model_DataReceived;
            Model.NoProtocol.Model_ErrorInReadThread += NoProtocol_Model_ErrorInReadThread;

            this.WhenAnyValue(x => x.TX_String)
                .WhereNotNull()
                .Where(x => x != String.Empty)
                .Subscribe(async _ =>
                {
                    try
                    {
                        if (Model.HostIsConnect &&
                            TX_String != String.Empty &&
                            TypeOfSendMessage == SendMessageType.Char)
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
                TypeOfSendMessage = SendMessageType.Char;
                TX_String = String.Empty;
            });

            Command_Select_String = ReactiveCommand.Create(() =>
            {
                TypeOfSendMessage = SendMessageType.String;
                TX_String = String.Empty;
            });

            Command_Send = ReactiveCommand.CreateFromTask(async () =>
            {
                await Model.NoProtocol.Send(TX_String, CR_Enable, LF_Enable);
            });

            Command_Send.ThrownExceptions.Subscribe(error => Message.Invoke("Ошибка отправки данных.\n\n" + error.Message, MessageType.Error));

            Command_ClearRX = ReactiveCommand.Create(() => { RX.Clear(); RX_String = RX.ToString(); });
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

            UI_IsEnable = true;
        }

        private void Model_DeviceIsDisconnected(object? sender, ConnectArgs e)
        {
            InterfaceType = InterfaceType_Default;

            UI_IsEnable = false;
        }

        private void NoProtocol_Model_DataReceived(object? sender, string e)
        {
            if (RX_NextLine)
            {
                e += "\n";
            }
            
            if (RX.Length + e.Length > RX.MaxCapacity)
            {
                RX.Remove(0, RX.Length + e.Length - RX.MaxCapacity);
            }

            RX.Append(e);

            RX_String = RX.ToString();
        }

        private void NoProtocol_Model_ErrorInReadThread(object? sender, string e)
        {
            Message.Invoke(e, MessageType.Error);
        }
    }
}
