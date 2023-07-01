using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Models.Settings
{
    public class Model_Settings
    {
        public DeviceData? Settings { get; private set; }

        private static Model_Settings? _model;

        public static Model_Settings Model
        {
            get => _model ?? (_model = new Model_Settings());
        }

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

        public void Save(string FileName, DeviceData Data)
        {
            try
            {
                if (FileName == string.Empty)
                {
                    throw new Exception("Не задано имя файла настроек.");
                }

                string FilePath = FolderPath_Settings + FileName + FileType;

                if (File.Exists(FilePath) == false)
                {
                    File.Create(FilePath).Close();
                }

                File.WriteAllText(FilePath, string.Empty);

                var Options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                using (FileStream Stream = new FileStream(FilePath, FileMode.Open))
                {
                    JsonSerializer.Serialize(Stream, Data, Options);
                }

                Settings = (DeviceData)Data.Clone();
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка сохранения настроек. " +
                    error.Message);
            }
        }

        public DeviceData Read(string FileName)
        {
            try
            {
                if (FileName == string.Empty)
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

                // Создание настроек по умолчанию, если в файле некоректные данные.
                catch (JsonException)
                {
                    Data = GetDefault();

                    Save(FileName, Data);
                }

                Settings = (DeviceData)Data.Clone();

                return Settings;
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка чтения данных из документа. " +
                    "Проверьте его целостность или выберите другой файл настроек. " +
                    "Возможно данный файл не совместим с текущей версией программы.\n\n" +
                    error.Message);
            }
        }

        public string[] FindFilesOfPresets()
        {
            if (Directory.Exists(FolderPath_Settings) == false)
            {
                Directory.CreateDirectory(FolderPath_Settings);
            }

            string[] ArrayOfPresets;

            ArrayOfPresets = Directory.GetFiles(FolderPath_Settings, "*" + FileType);

            // Создание файла настроек по умолчанию, если в папке нет ни одного файла.
            if (ArrayOfPresets.Length == 0)
            {
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
