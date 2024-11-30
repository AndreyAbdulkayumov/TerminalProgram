using Core.Clients;
using Core.Models.Modbus;
using Core.Models.NoProtocol;
using Core.Models.Settings;
using System.Text;

namespace Core.Models
{
    public class ConnectArgs : EventArgs
    {
        public readonly IConnection? ConnectedDevice;

        public ConnectArgs(IConnection? connectedDevice)
        {
            ConnectedDevice = connectedDevice;
        }
    }

    public class ConnectedHost
    {
        public bool HostIsConnect
        {
            get
            {
                if (Client == null)
                {
                    return false;
                }

                return Client.IsConnected;
            }
        }

        public int Host_ReadTimeout
        {
            get
            {
                if (Client == null)
                {
                    return 0;
                }

                return Client.ReadTimeout;
            }
        }

        public event EventHandler<ConnectArgs>? DeviceIsConnect;
        public event EventHandler<ConnectArgs>? DeviceIsDisconnected;

        // Реализация паттерна "Одиночка"
        private static ConnectedHost? _model;

        public static ConnectedHost Model
        {
            get => _model ?? (_model = new ConnectedHost());
        }

        public readonly Model_NoProtocol NoProtocol;
        public readonly Model_Modbus Modbus;

        public IConnection? Client { get; private set; }

        public static ProtocolMode? SelectedProtocol { get; private set; }

        // Значение кодировки по умолчанию
        public static Encoding GlobalEncoding { get; private set; } = Encoding.Default;


        public ConnectedHost()
        {
            NoProtocol = new Model_NoProtocol(this);
            Modbus = new Model_Modbus(this);

            SetProtocol_NoProtocol();

            DeviceIsDisconnected?.Invoke(this, new ConnectArgs(Client));
        }

        public void SetProtocol_NoProtocol()
        {
            SelectedProtocol = new ProtocolMode_NoProtocol(Client);
        }

        public void SetProtocol_Modbus()
        {
            SelectedProtocol = new ProtocolMode_Modbus(Client, Model_Settings.Model.Settings);
        }

        public void SetGlobalEncoding(Encoding globalEncoding)
        {
            GlobalEncoding = globalEncoding;
        }

        public void Connect(ConnectionInfo information)
        {
            if (SelectedProtocol == null)
            {
                throw new Exception("Не выбран протокол.");
            }

            if (information.Info as SerialPortInfo != null)
            {
                Client = new SerialPortClient();
            }

            else if (information.Info as SocketInfo != null)
            {
                Client = new IPClient();
            }

            else
            {
                throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
            }

            Client.Connect(information);

            SetGlobalEncoding(GlobalEncoding);

            var modbusProtocol = SelectedProtocol as ProtocolMode_Modbus;

            modbusProtocol?.UpdateTimeouts(Model_Settings.Model.Settings);

            SelectedProtocol.InitMode(Client);

            DeviceIsConnect?.Invoke(this, new ConnectArgs(Client));
        }

        public async Task Disconnect()
        {
            if (Client == null)
            {
                return;
            }

            await Client.Disconnect();

            DeviceIsDisconnected?.Invoke(this, new ConnectArgs(Client));
        }        
    }
}
