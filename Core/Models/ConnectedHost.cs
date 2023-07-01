using Core.Models.Modbus;
using Core.Models.NoProtocol;
using Core.Models.Settings;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class ConnectArgs : EventArgs
    {
        public IConnection? ConnectedDevice;

        public ConnectArgs(IConnection? ConnectedDevice)
        {
            this.ConnectedDevice = ConnectedDevice;
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

        private IConnection? Client;

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

        public void Connect(ConnectionInfo Information)
        {
            if (SelectedProtocol == null)
            {
                throw new Exception("Не выбран протокол.");
            }

            if (Information.Info as SerialPortInfo != null)
            {
                Client = new SerialPortClient();
                
            }

            else if (Information.Info as SocketInfo != null)
            {
                Client = new IPClient();
            }

            else
            {
                throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
            }

            Client.Connect(Information);

            GlobalEncoding = Information.GlobalEncoding;

            ProtocolMode_Modbus? ModbusProtocol = SelectedProtocol as ProtocolMode_Modbus;

            ModbusProtocol?.UpdateTimeouts(Model_Settings.Model.Settings);

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
