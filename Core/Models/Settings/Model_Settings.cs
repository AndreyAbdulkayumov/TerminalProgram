using System.Text.Json;

namespace Core.Models.Settings
{
    public class Model_Settings
    {
        public DeviceData? Settings { get; private set; }

        public AppInfo AppData { get; private set; }

        private static Model_Settings? _model;

        public static Model_Settings Model
        {
            get => _model ?? (_model = new Model_Settings());
        }

        // Путь к папке с файлами настроек
        public string FolderPath_Settings
        {
            get => DirectoryManager.SettingsFiles_Directory;
        }

        private const string FileName_DefaultPreset = "Unknown";

        private const string FileName_AppData = "AppData";

        private const string FileExtension = ".json";

        private readonly AppDirectoryManager DirectoryManager = new AppDirectoryManager();

        public Model_Settings()
        {
            AppData = ReadAppInfo();
        }

        /// <summary>
        /// Сохранение данных в файл. Если файл с таким именем не найден, то он создается.
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="Data"></param>
        /// <exception cref="Exception"></exception>
        public void Save(string FileName, DeviceData Data)
        {
            try
            {
                if (FileName == string.Empty)
                {
                    throw new Exception("Не задано имя файла настроек.");
                }

                string FilePath = Path.Combine(FolderPath_Settings, FileName + FileExtension);

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
                throw new Exception("Ошибка сохранения настроек.\n\n" + error.Message);
            }
        }

        /// <summary>
        /// Чтение настроек из файла. Если файл содержит битые данные, то он перезаписывается значениями по умолчанию.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DeviceData Read(string FileName)
        {
            try
            {
                if (FileName == string.Empty)
                {
                    throw new Exception("Не задано имя файла настроек.");
                }

                string FilePath = Path.Combine(FolderPath_Settings, FileName + FileExtension);

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
                        Data = DeviceData.GetDefault();

                        Save(FileName, Data);
                    }
                }

                // Создание настроек по умолчанию, если в файле некоректные данные.
                catch (JsonException)
                {
                    Data = DeviceData.GetDefault();

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

        /// <summary>
        /// Удаляет файл по указанному пути, если он существует.
        /// </summary>
        /// <param name="FileName"></param>
        public void Delete(string FileName)
        {
            string[] ArrayOfFiles = Directory.GetFiles(DirectoryManager.SettingsFiles_Directory);

            string SelectedFile_FullPath = Path.Combine(DirectoryManager.SettingsFiles_Directory, FileName + FileExtension);

            if (ArrayOfFiles.Contains(SelectedFile_FullPath))
            {
                File.Delete(Path.Combine(DirectoryManager.SettingsFiles_Directory, FileName) + FileExtension);
            }

            else
            {
                throw new Exception("Не удалось найти файл \"" + FileName + "\" в папке " + DirectoryManager.SettingsFiles_Directory);
            }
        }

        /// <summary>
        /// Копирует файл с указанным путем в директорию приложения.
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns>Имя скопированного файла без расширения.</returns>
        public string CopyFrom(string FilePath)
        {
            string destFilePath = Path.Combine(DirectoryManager.SettingsFiles_Directory, Path.GetFileName(FilePath));

            string FileName = Path.GetFileNameWithoutExtension(FilePath);

            File.Copy(FilePath, destFilePath);

            return FileName;
        }

        /// <summary>
        /// Возвращает имена всех доступных файлов настроек.
        /// </summary>
        /// <returns></returns>
        public string[] FindFilesOfPresets()
        {
            string[] ArrayOfPresets = DirectoryManager.CheckFiles(
                DirectoryManager.SettingsFiles_Directory, 
                FileName_DefaultPreset, 
                FileExtension, 
                DeviceData.GetDefault()
                );

            return ArrayOfPresets;
        }

        public void SaveAppInfo(AppInfo Data)
        {
            string FilePath = Path.Combine(DirectoryManager.CommonFiles_Directory, FileName_AppData + FileExtension);

            try
            {
                if (File.Exists(FilePath) == false)
                {
                    File.Create(FilePath).Close();
                }

                File.WriteAllText(FilePath, string.Empty);

                var Options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                using (FileStream Stream = new FileStream(FilePath, FileMode.OpenOrCreate))
                {
                    JsonSerializer.Serialize(Stream, Data, Options);
                }

                AppData = Data;
            }

            catch (Exception error)
            {
                throw new Exception("Не удалось записать данные в файл настроек приложения.\n\n" + error.Message);
            }
        }

        private AppInfo ReadAppInfo()
        {
            string FilePath = DirectoryManager.FindOrCreateFile(
                DirectoryManager.CommonFiles_Directory,
                FileName_AppData,
                FileExtension,
                AppInfo.GetDefault(FileName_DefaultPreset)
                );

            AppInfo? Data;

            try
            {
                using (FileStream Stream = new FileStream(FilePath, FileMode.Open))
                {
                    Data = JsonSerializer.Deserialize<AppInfo>(Stream);
                }

                if (Data == null)
                {
                    SaveAppInfo(AppInfo.GetDefault(FileName_DefaultPreset));

                    Data = AppInfo.GetDefault(FileName_DefaultPreset);
                }
            }

            // Создание настроек по умолчанию, если в файле некоректные данные.
            catch (JsonException)
            {
                SaveAppInfo(AppInfo.GetDefault(FileName_DefaultPreset));

                Data = AppInfo.GetDefault(FileName_DefaultPreset);
            }

            return Data;
        }
    }
}
