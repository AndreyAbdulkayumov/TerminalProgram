using Core.Models.Modbus;
using Core.Models.NoProtocol;
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
        public IConnection ConnectedDevice;

        public ConnectArgs(IConnection ConnectedDevice)
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

        private IConnection? Client = null;

        public ProtocolMode? SelectedProtocol { get; private set; }

        public DeviceData Settings { get; private set; } = new DeviceData();

        // Значение кодировки по умолчанию
        public static Encoding GlobalEncoding { get; private set; } = Encoding.Default;


        


        public ConnectedHost()
        {
            NoProtocol = new Model_NoProtocol(this);
            Modbus = new Model_Modbus();

            SetProtocol_NoProtocol();
        }


        public void SetProtocol_NoProtocol()
        {
            SelectedProtocol = new ProtocolMode_NoProtocol(Client);
        }

        public void SetProtocol_Modbus()
        {
            SelectedProtocol = new ProtocolMode_Modbus(Client, Settings);
        }
                

        public async Task Connect(string FileName)
        {
            if (SelectedProtocol == null)
            {
                throw new Exception("Не выбран протокол.");
            }

            await ReadSettings(FileName);

            switch (Settings.TypeOfConnection)
            {
                case "SerialPort":

                    Client = new SerialPortClient();

                    Client.Connect(new ConnectionInfo(new SerialPortInfo(
                        Settings.Connection_SerialPort?.COMPort,
                        Settings.Connection_SerialPort?.BaudRate_IsCustom == "Enable" ?
                           Settings.Connection_SerialPort?.BaudRate_Custom : Settings.Connection_SerialPort?.BaudRate,
                        Settings.Connection_SerialPort?.Parity,
                        Settings.Connection_SerialPort?.DataBits,
                        Settings.Connection_SerialPort?.StopBits
                        ),
                        GlobalEncoding));

                    break;

                case "Ethernet":

                    Client = new IPClient();

                    Client.Connect(new ConnectionInfo(new SocketInfo(
                        Settings.Connection_IP?.IP_Address,
                        Settings.Connection_IP?.Port
                        ),
                        GlobalEncoding));

                    break;

                default:
                    throw new Exception("В файле настроек задан неизвестный интерфейс связи.");
            }

            ProtocolMode_Modbus? ModbusProtocol = SelectedProtocol as ProtocolMode_Modbus;

            ModbusProtocol?.UpdateTimeouts(Settings);

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


        private async Task ReadSettings(string DocumentName)
        {
            try
            {
                SystemOfSettings.Settings_FileName = DocumentName;

                DeviceData Device = await SystemOfSettings.Read();

                Settings = (DeviceData)Device.Clone();

                if (Settings.GlobalEncoding == null)
                {
                    throw new Exception("Не удалось обработать значение кодировки.");
                }

                GlobalEncoding = GetEncoding(Settings.GlobalEncoding);
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка чтения данных из документа. " +
                    "Проверьте его целостность или выберите другой файл настроек. " +
                    "Возможно данный файл не совместим с текущей версией программы.\n\n" +
                    error.Message);
            }
        }

        private Encoding GetEncoding(string EncodingName)
        {
            switch (EncodingName)
            {
                case "ASCII":
                    return Encoding.ASCII;

                case "Unicode":
                    return Encoding.Unicode;

                case "UTF-32":
                    return Encoding.UTF32;

                case "UTF-8":
                    return Encoding.UTF8;

                default:
                    throw new Exception("Задан неизвестный тип кодировки: " + EncodingName);
            }
        }
    }
}
