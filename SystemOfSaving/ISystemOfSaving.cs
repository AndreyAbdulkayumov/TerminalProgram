using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOfSaving
{
    public enum TypeOfDeviceData
    {
        TimeoutWrite,
        TimeoutWrite_IsInfinite,
        TimeoutRead,
        TimeoutRead_IsInfinite,

        GlobalEncoding,

        TypeOfConnection,

        COMPort,
        BaudRate,
        Parity,
        DataBits,
        StopBits,

        IP,
        Port,
    }

    public class DeviceData
    {
        public string DeviceName;

        public string TimeoutWrite;
        public string TimeoutWrite_IsInfinite;
        public string TimeoutRead;
        public string TimeoutRead_IsInfinite;

        public string GlobalEncoding;

        public string TypeOfConnection;

        // Serial Port
        public string COMPort;
        public string BaudRate;
        public string Parity;
        public string DataBits;
        public string StopBits;

        // Socket
        public string IP;
        public string Port;
    }

    interface ISystemOfSaving
    {
        void SetParameter(string DeviceName, TypeOfDeviceData TypeOfData, string Value);
        string GetParameter(string DeviceName, TypeOfDeviceData TypeOfData);
        void CreateDeviceSettings(string Name);
        void SaveSettings(DeviceData Data);
        DeviceData GetDeviceData(string DeviceName);
        List<string> GetDevicesNames();
        void RenameDevice(string CurrentDeviceName, string NewDeviceName);
        void RemoveDevice(string DeviceName);
    }
}
