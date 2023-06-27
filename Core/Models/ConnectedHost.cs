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

        public static ProtocolMode? SelectedProtocol { get; private set; }

        public DeviceData Settings { get; private set; } = new DeviceData();

        // Значение кодировки по умолчанию
        public static Encoding GlobalEncoding { get; private set; } = Encoding.Default;





        public ConnectedHost()
        {
            NoProtocol = new Model_NoProtocol(this);
            Modbus = new Model_Modbus();

            SetProtocol_NoProtocol();

            DeviceIsDisconnected?.Invoke(this, new ConnectArgs(Client));
        }


        public void SetProtocol_NoProtocol()
        {
            SelectedProtocol = new ProtocolMode_NoProtocol(Client);
        }

        public void SetProtocol_Modbus()
        {
            SelectedProtocol = new ProtocolMode_Modbus(Client, Settings);
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


        public string[] GetSettings_FileNames()
        {
            return SystemOfSettings.FindFilesOfPresets();
        }

        public void SaveSettings(string DocumentName, DeviceData Data)
        {
            try
            {
                SystemOfSettings.Save(DocumentName, Data);

                Settings = (DeviceData)Data.Clone();
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка сохранения настроек. " +
                    error.Message);
            }
        }

        public void ReadSettings(string DocumentName)
        {
            try
            {
                DeviceData Device = SystemOfSettings.Read(DocumentName);

                Settings = (DeviceData)Device.Clone();

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

        public Encoding GetEncoding(string? EncodingName)
        {
            if (Settings.GlobalEncoding == null)
            {
                throw new Exception("Не удалось обработать значение кодировки.");
            }


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
