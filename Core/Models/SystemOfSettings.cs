using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;

namespace Core.Models
{
    public static class SystemOfSettings
    {
        public const string FolderPath_Settings = "Settings/";

        private const string DefaultFileName = "Unknown";

        public const string FileType = ".json";

        private static DeviceData DefaultSettings = new DeviceData()
        {
            GlobalEncoding = "UTF-8",

            TimeoutWrite = "300",
            TimeoutRead = "300",
            
            TypeOfConnection = DeviceData.ConnectionName_SerialPort,

            Connection_SerialPort = new SerialPort_Info()
            {
                COMPort = null,
                BaudRate = null,
                BaudRate_IsCustom = false,
                BaudRate_Custom = null,
                Parity = null,
                DataBits = null,
                StopBits = null
            },

            Connection_IP = new IP_Info()
            {
                IP_Address = null,
                Port = null
            }
        };

        public static DeviceData GetDefault()
        {
            return (DeviceData)DefaultSettings.Clone();
        }

        public static void Save(string FileName, DeviceData Settings)
        {
            if (FileName == String.Empty)
            {
                throw new Exception("Не задано имя файла настроек.");
            }

            string FilePath = FolderPath_Settings + FileName + FileType;

            if (File.Exists(FilePath) == false)
            {
                File.Create(FilePath).Close();
            }

            File.WriteAllText(FilePath, String.Empty);

            var Options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            using (FileStream Stream = new FileStream(FilePath, FileMode.Open))
            {
                JsonSerializer.Serialize(Stream, Settings, Options);
            }
        }

        public static DeviceData Read(string FileName)
        {
            if (FileName == String.Empty)
            {
                throw new Exception("Не задано имя файла настроек.");
            }

            string FilePath = FolderPath_Settings + FileName + FileType;

            if (File.Exists(FilePath) == false)
            {
                throw new Exception("Файл настроек не существует.\n\n" + "Путь: " + FilePath);
            }

            DeviceData? Data;            

            try
            {
                using (FileStream Stream = new FileStream(FilePath, FileMode.Open))
                {
                    Data = JsonSerializer.Deserialize<DeviceData>(Stream);
                }
                
                if (Data == null)
                {
                    Data = GetDefault();

                    Save(FileName, Data);
                }
            }

            catch (JsonException)
            {
                Data = GetDefault();

                Save(FileName, Data);

                //MessageBox.Show("В файле заданы некоректные данные. Созданы настройки по умолчанию.",
                //    "Предупреждение", MessageBoxButton.OK,
                //    MessageBoxImage.Warning, MessageBoxResult.OK);
            }

            return Data;
        }

        public static string[] FindFilesOfPresets()
        {
            if (Directory.Exists(FolderPath_Settings) == false)
            {
                Directory.CreateDirectory(FolderPath_Settings);
            }

            string[] ArrayOfPresets;

            ArrayOfPresets = Directory.GetFiles(FolderPath_Settings, "*" + FileType);

            if (ArrayOfPresets.Length == 0)
            {
                //MessageBox.Show("Не найдено ни одного файла настроек. Будет создан файл с настройками по умолчанию.",
                //    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                string DefaultFilePath = FolderPath_Settings + DefaultFileName + FileType;

                File.Create(DefaultFilePath).Close();

                Save(DefaultFileName, GetDefault());

                ArrayOfPresets = Directory.GetFiles(FolderPath_Settings, "*" + FileType);
            }

            for (int i = 0; i < ArrayOfPresets.Length; i++)
            {
                ArrayOfPresets[i] = Path.GetFileNameWithoutExtension(ArrayOfPresets[i]);
            }

            return ArrayOfPresets;
        }
    }
}
