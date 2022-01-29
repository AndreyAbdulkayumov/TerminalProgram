using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemOfSaving.DocumentXML;

namespace SystemOfSaving
{
    public class SettingsMediator
    {
        public string DefaultNodeValue { get; } = "Empty";

        private readonly XmlWorker Document;

        public SettingsMediator()
        {
            Document = new XmlWorker(DefaultNodeValue);
        }

        public void LoadSettingsFrom(string DocumentPath)
        {
            Document.Load(DocumentPath);
        }

        public void Save(DeviceData Data)
        {
            Document.SaveSettings(Data);
        }

        public void CreateDevice(string Name)
        {
            Document.CreateDeviceSettings(Name);
        }

        public void RemoveDevice(string Name)
        {
            Document.RemoveDevice(Name);
        }

        public List<string> GetAllDevicesNames()
        {
            return Document.GetDevicesNames();
        }

        public void RenameDevice(string CurrentName, string NewName)
        {
            Document.RenameDevice(CurrentName, NewName);
        }

        public DeviceData GetDeviceData(string DeviceName)
        {
            return Document.GetDeviceData(DeviceName);
        }
    }
}
