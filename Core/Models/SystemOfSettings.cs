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
        private readonly static string FolderPath_Settings = "Settings/";

        public static string Settings_FilePath
        {
            get { return FolderPath_Settings + Settings_FileName + FileType; }
        }

        public static string? Settings_FileName { get; set; }

        private const string DefaultFileName = "Unknown";

        public const string FileType = ".json";

        private static DeviceData DefaultSettings = new DeviceData()
        {
            GlobalEncoding = "UTF-8",

            TimeoutWrite = "300",
            TimeoutRead = "300",
            
            TypeOfConnection = "SerialPort",

            Connection_SerialPort = new SerialPort_Info()
            {
                COMPort = null,
                BaudRate = null,
                BaudRate_IsCustom = "Disable",
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

        public static async Task Save(DeviceData Settings)
        {
            if (Settings_FilePath == null || Settings_FilePath == String.Empty)
            {
                throw new Exception("Не задан путь к файлу настроек.");
            }

            if (File.Exists(Settings_FilePath) == false)
            {
                File.Create(Settings_FilePath).Close();
            }

            File.WriteAllText(Settings_FilePath, String.Empty);

            var Options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            using (FileStream Stream = new FileStream(Settings_FilePath, FileMode.Open))
            {
                await JsonSerializer.SerializeAsync(Stream, Settings, Options);
            }
        }

        public static async Task<DeviceData> Read()
        {
            if (Settings_FilePath == null || Settings_FilePath == String.Empty)
            {
                throw new Exception("Не задан путь к файлу настроек.");
            }

            if (File.Exists(Settings_FilePath) == false)
            {
                throw new Exception("Файл настроек не существует.\n\n" + "Путь: " + Settings_FilePath);
            }

            DeviceData? Data;            

            try
            {
                using (FileStream Stream = new FileStream(Settings_FilePath, FileMode.Open))
                {
                    Data = await JsonSerializer.DeserializeAsync<DeviceData>(Stream);
                }
                
                if (Data == null)
                {
                    Data = GetDefault();

                    await Save(Data);
                }
            }

            catch (JsonException)
            {
                Data = GetDefault();

                await Save(Data);

                //MessageBox.Show("В файле заданы некоректные данные. Созданы настройки по умолчанию.",
                //    "Предупреждение", MessageBoxButton.OK,
                //    MessageBoxImage.Warning, MessageBoxResult.OK);
            }

            return Data;
        }

        public static async Task<string[]> FindFilesOfPresets()
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

                string NewFilePath = FolderPath_Settings + DefaultFileName + FileType;

                File.Create(NewFilePath).Close();

                Settings_FileName = DefaultFileName;

                await Save(GetDefault());

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
