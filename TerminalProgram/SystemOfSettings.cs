using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Security.AccessControl;

namespace TerminalProgram
{
    public static class SystemOfSettings
    {
        public static string Settings_FilePath { get; set; }

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

            DeviceData Data;

            using (FileStream Stream = new FileStream(Settings_FilePath, FileMode.Open))
            {
                Data = await JsonSerializer.DeserializeAsync<DeviceData>(Stream);
            }

            return Data;
        }
    }
}
