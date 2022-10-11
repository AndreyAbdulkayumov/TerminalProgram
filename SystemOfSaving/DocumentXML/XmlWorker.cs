using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;

namespace SystemOfSaving.DocumentXML
{
    public class XmlWorker : ISystemOfSaving
    {
        private string SettingsDocumentPath;

        private readonly XmlDocument SettingsDocument = new XmlDocument();

        private string DefaultNodeValue;

        public XmlWorker(string DefaultValue)
        {
            DefaultNodeValue = DefaultValue;
        }

        public void Load(string DocumentPath)
        {
            try
            {
                SettingsDocumentPath = DocumentPath;
                
                SettingsDocument.Load(SettingsDocumentPath);
            }
            catch (Exception error)
            {
                throw new Exception("Попытка загрузить файл настроек: \"" + SettingsDocumentPath +
                    "\".\n\nВозможно такого файла не существует.\n\n" + error.Message);
            }
        }

        public void SetParameter(string DeviceName, TypeOfDeviceData TypeOfData, string Value)
        {
            XmlElement Root = SettingsDocument.DocumentElement;

            XmlNode DeviceNode = XML_FindNode.InElement(Root, DeviceName);

            XmlNode ParameterNode = XML_FindNode.InNode(DeviceNode, TypeOfData.ToString());

            UpdateDataNode(SettingsDocument, ParameterNode, Value);

            SettingsDocument.Save(SettingsDocumentPath);
        }

        public string GetParameter(string DeviceName, TypeOfDeviceData TypeOfData)
        {
            XmlElement Root = SettingsDocument.DocumentElement;

            XmlNode DeviceNode = XML_FindNode.InElement(Root, DeviceName);

            XmlNode ParameterNode = XML_FindNode.InNode(DeviceNode, TypeOfData.ToString());

            return ParameterNode.FirstChild.Value;
        }

        public void CreateDeviceSettings(string Name)
        {
            XmlElement Root = SettingsDocument.DocumentElement;

            DeviceData DefaultData = new DeviceData
            {
                DeviceName = Name,

                TimeoutWrite = DefaultNodeValue,
                TimeoutWrite_IsInfinite = DefaultNodeValue,
                TimeoutRead = DefaultNodeValue,
                TimeoutRead_IsInfinite = DefaultNodeValue,

                GlobalEncoding = DefaultNodeValue,

                TypeOfConnection = DefaultNodeValue,

                COMPort = DefaultNodeValue,
                BaudRate = DefaultNodeValue,
                Parity = DefaultNodeValue,
                DataBits = DefaultNodeValue,
                StopBits = DefaultNodeValue,

                IP = DefaultNodeValue,
                Port = DefaultNodeValue,
            };

            CreateBlock(DefaultData, SettingsDocument, Root);

            SettingsDocument.Save(SettingsDocumentPath);
        }

        public void SaveSettings(DeviceData Data)
        {
            ConvertStructToXML(Data, SettingsDocument, SettingsDocumentPath);
        }

        public DeviceData GetDeviceData(string DeviceName)
        {
            XmlElement Root = SettingsDocument.DocumentElement;

            XmlNode DeviceNode = XML_FindNode.InElement(Root, DeviceName);

            DeviceData Data = new DeviceData
            {
                DeviceName = DeviceNode.Attributes.GetNamedItem("name").Value,

                TimeoutWrite = XML_FindNode.InNode(DeviceNode, "TimeoutWrite").FirstChild.Value,
                TimeoutWrite_IsInfinite = XML_FindNode.InNode(DeviceNode, "TimeoutWrite_IsInfinite").FirstChild.Value,
                TimeoutRead = XML_FindNode.InNode(DeviceNode, "TimeoutRead").FirstChild.Value,
                TimeoutRead_IsInfinite = XML_FindNode.InNode(DeviceNode, "TimeoutRead_IsInfinite").FirstChild.Value,

                GlobalEncoding = XML_FindNode.InNode(DeviceNode, "GlobalEncoding").FirstChild.Value,

                TypeOfConnection = XML_FindNode.InNode(DeviceNode, "TypeOfConnection").FirstChild.Value,              

                COMPort = XML_FindNode.InNode(DeviceNode, "COMPort").FirstChild.Value,
                BaudRate = XML_FindNode.InNode(DeviceNode, "BaudRate").FirstChild.Value,
                Parity = XML_FindNode.InNode(DeviceNode, "Parity").FirstChild.Value,
                DataBits = XML_FindNode.InNode(DeviceNode, "DataBits").FirstChild.Value,
                StopBits = XML_FindNode.InNode(DeviceNode, "StopBits").FirstChild.Value,

                IP = XML_FindNode.InNode(DeviceNode, "IP").FirstChild.Value,
                Port = XML_FindNode.InNode(DeviceNode, "Port").FirstChild.Value,

            };

            return Data;
        }

        public List<string> GetDevicesNames()
        {
            SettingsDocument.Load(SettingsDocumentPath);

            XmlElement Root = SettingsDocument.DocumentElement;
            List<string> DevicesNames = new List<string>();

            foreach (XmlNode Node in Root)
            {
                if (Node.Attributes.Count > 0)
                {
                    XmlNode attribute = Node.Attributes.GetNamedItem("name");

                    if (attribute != null)
                    {
                        DevicesNames.Add(attribute.Value);
                    }
                }
            }

            return DevicesNames;
        }

        public void RenameDevice(string CurrentDeviceName, string NewDeviceName)
        {
            XmlElement Root = SettingsDocument.DocumentElement;

            foreach (XmlNode Node in Root)
            {
                if (Node.Attributes.Count > 0)
                {
                    XmlNode attribute = Node.Attributes.GetNamedItem("name");

                    if (attribute != null && attribute.Value == CurrentDeviceName)
                    {
                        attribute.Value = NewDeviceName;
                        SettingsDocument.Save(SettingsDocumentPath);
                        return;
                    }
                }
            }

            throw new Exception("Не найдено устройство с именем \"" + CurrentDeviceName + "\".");
        }

        public void RemoveDevice(string DeviceName)
        {
            XmlElement Root = SettingsDocument.DocumentElement;

            foreach (XmlNode Node in Root)
            {
                if (Node.Attributes.Count > 0)
                {
                    XmlNode attribute = Node.Attributes.GetNamedItem("name");

                    if (attribute != null && attribute.Value == DeviceName)
                    {
                        Root.RemoveChild(Node);
                    }
                }

            }

            SettingsDocument.Save(SettingsDocumentPath);
        }


        /******************************************/
        //
        // Common Methods
        //
        /******************************************/



        private void ConvertStructToXML(DeviceData Data, XmlDocument Document, string DocumentPath)
        {
            XmlElement Root = Document.DocumentElement;

            if (Root.FirstChild == null)
            {
                CreateBlock(Data, Document, Root);
            }

            else
            {
                foreach (XmlNode Node in Root)
                {
                    if (Node.Attributes.Count > 0)
                    {
                        XmlNode attribute = Node.Attributes.GetNamedItem("name");

                        if (attribute != null && attribute.Value == Data.DeviceName)
                        {
                            UpdateDeviceNode(Data, Document, Node);
                        }
                    }

                    else
                    {
                        throw new Exception("Файл настроек пуст");
                    }
                }
            }

            Document.Save(DocumentPath);
        }

        private void UpdateDeviceNode(DeviceData Data, XmlDocument Document, XmlNode SelectedNode)
        {
            foreach (XmlNode DataNode in SelectedNode)
            {
                switch (DataNode.LocalName)
                {
                    case "TimeoutWrite":
                        UpdateDataNode(Document, DataNode, Data.TimeoutWrite);
                        break;

                    case "TimeoutWrite_IsInfinite":
                        UpdateDataNode(Document, DataNode, Data.TimeoutWrite_IsInfinite);
                        break;

                    case "TimeoutRead":
                        UpdateDataNode(Document, DataNode, Data.TimeoutRead);
                        break;

                    case "TimeoutRead_IsInfinite":
                        UpdateDataNode(Document, DataNode, Data.TimeoutRead_IsInfinite);
                        break;


                    case "GlobalEncoding":
                        UpdateDataNode(Document, DataNode, Data.GlobalEncoding);
                        break;


                    case "TypeOfConnection":
                        UpdateDataNode(Document, DataNode, Data.TypeOfConnection);
                        break;



                    case "COMPort":
                        UpdateDataNode(Document, DataNode, Data.COMPort);
                        break;

                    case "BaudRate":
                        UpdateDataNode(Document, DataNode, Data.BaudRate);
                        break;

                    case "Parity":
                        UpdateDataNode(Document, DataNode, Data.Parity);
                        break;

                    case "DataBits":
                        UpdateDataNode(Document, DataNode, Data.DataBits);
                        break;

                    case "StopBits":
                        UpdateDataNode(Document, DataNode, Data.StopBits);
                        break;



                    case "IP":
                        UpdateDataNode(Document, DataNode, Data.IP);
                        break;

                    case "Port":
                        UpdateDataNode(Document, DataNode, Data.Port);
                        break;

                }
            }
        }

        private void CreateBlock(DeviceData Data, XmlDocument Document, XmlElement Root)
        {
            XmlElement Device = Document.CreateElement("Device");
            XmlAttribute DeviceNameAttr = Document.CreateAttribute("name");
            XmlText DeviceNameText = Document.CreateTextNode(Data.DeviceName);

            DeviceNameAttr.AppendChild(DeviceNameText);
            Device.Attributes.Append(DeviceNameAttr);

            CreateValue(Document, Device, "TimeoutWrite", Data.TimeoutWrite);
            CreateValue(Document, Device, "TimeoutWrite_IsInfinite", Data.TimeoutWrite_IsInfinite);
            CreateValue(Document, Device, "TimeoutRead", Data.TimeoutRead);
            CreateValue(Document, Device, "TimeoutRead_IsInfinite", Data.TimeoutRead_IsInfinite);

            CreateValue(Document, Device, "GlobalEncoding", Data.GlobalEncoding);

            CreateValue(Document, Device, "TypeOfConnection", Data.TypeOfConnection);

            CreateValue(Document, Device, "COMPort", Data.COMPort);
            CreateValue(Document, Device, "BaudRate", Data.BaudRate);
            CreateValue(Document, Device, "Parity", Data.Parity);
            CreateValue(Document, Device, "DataBits", Data.DataBits);
            CreateValue(Document, Device, "StopBits", Data.StopBits);

            CreateValue(Document, Device, "IP", Data.IP);
            CreateValue(Document, Device, "Port", Data.Port);

            Root.AppendChild(Device);
        }


        private void UpdateDataNode(XmlDocument Document, XmlNode DataNode, string Data)
        {
            if (Data == String.Empty || Data == null)
            {
                if (DataNode.FirstChild == null)
                {
                    XmlText DefaultText = Document.CreateTextNode(Data);
                    DataNode.AppendChild(DefaultText);
                }

                DataNode.FirstChild.Value = DefaultNodeValue;
            }

            else
            {
                if (DataNode.FirstChild == null)
                {
                    XmlText DefaultText = Document.CreateTextNode(Data);
                    DataNode.AppendChild(DefaultText);

                }

                DataNode.FirstChild.Value = Data;
            }
        }

        private void CreateValue(XmlDocument Document, XmlElement BasicElement, string NameOfElement, string Data)
        {
            XmlElement COMPortElem = Document.CreateElement(NameOfElement);
            XmlText COMPortText = Document.CreateTextNode(Data == null || Data == String.Empty ? DefaultNodeValue : Data);

            COMPortElem.AppendChild(COMPortText);
            BasicElement.AppendChild(COMPortElem);
        }
    }
}
