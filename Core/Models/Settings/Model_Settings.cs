using Core.Models.Settings.FileTypes;

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

        /// <summary>
        /// Путь к папке с файлами настроек
        /// </summary>
        public string FolderPath_Settings
        {
            get => DirectoryManager.SettingsFiles_Directory;
        }

        public string FilePath_Macros_NoProtocol
        {
            get => Path.Combine(DirectoryManager.Macros_Directory, FileName_Macros_NoProtocol + FileExtension);
        }

        public string FilePath_Macros_Modbus
        {
            get => Path.Combine(DirectoryManager.Macros_Directory, FileName_Macros_Modbus + FileExtension);
        }

        private const string FileName_DefaultPreset = "Unknown";

        private const string FileName_AppData = "AppData";

        private const string FileName_Macros_NoProtocol = "Macros_NoProtocol";
        private const string FileName_Macros_Modbus = "Macros_Modbus";

        private const string FileExtension = ".json";

        private readonly AppDirectoryManager DirectoryManager = new AppDirectoryManager();

        public Model_Settings()
        {
            AppData = ReadAppInfo();
        }

        /// <summary>
        /// Сохранение данных в файл. Если файл с таким именем не найден, то он создается.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <exception cref="Exception"></exception>
        public void SavePreset(string fileName, DeviceData data)
        {
            try
            {
                if (fileName == string.Empty)
                {
                    throw new Exception("Не задано имя файла настроек.");
                }

                string filePath = Path.Combine(FolderPath_Settings, fileName + FileExtension);

                FileIO.Save(filePath, data);

                Settings = (DeviceData)data.Clone();
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка сохранения настроек.\n\n" + error.Message);
            }
        }

        /// <summary>
        /// Чтение настроек из файла. Если файл содержит битые данные, то он перезаписывается значениями по умолчанию.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DeviceData ReadPreset(string fileName)
        {
            try
            {
                if (fileName == string.Empty)
                {
                    throw new Exception("Не задано имя файла настроек.");
                }

                string filePath = Path.Combine(FolderPath_Settings, fileName + FileExtension);

                Settings = FileIO.ReadOrCreateDefault(filePath, DeviceData.GetDefault());

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
        /// <param name="fileName"></param>
        public void DeleteFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception($"Файла \"{fileName}\" не существует.");
            }

            File.Delete(fileName);
        }

        /// <summary>
        /// Удаляет файл пресета по указанному пути, если он существует.
        /// </summary>
        /// <param name="fileName"></param>
        public void DeletePreset(string fileName)
        {
            string[] arrayOfFiles = Directory.GetFiles(DirectoryManager.SettingsFiles_Directory);

            string selectedFile_FullPath = Path.Combine(DirectoryManager.SettingsFiles_Directory, fileName + FileExtension);

            if (arrayOfFiles.Contains(selectedFile_FullPath))
            {
                File.Delete(Path.Combine(DirectoryManager.SettingsFiles_Directory, fileName) + FileExtension);
            }

            else
            {
                throw new Exception("Не удалось найти файл \"" + fileName + "\" в папке " + DirectoryManager.SettingsFiles_Directory);
            }
        }

        /// <summary>
        /// Копирование файла из одной директории в другую.
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="destFileName"></param>
        public void CopyFile(string sourceFileName, string destFileName)
        {
            if (File.Exists(destFileName))
            {
                throw new Exception($"Файл с именем \"{Path.GetFileName(destFileName)}\" уже существует в директории \"{Path.GetDirectoryName(destFileName)}\".");
            }

            File.Copy(sourceFileName, destFileName);
        }

        /// <summary>
        /// Копирует файл с указанным путем в директорию пресетов.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Имя скопированного файла без расширения.</returns>
        public string CopyInPresetFolderFrom(string filePath)
        {
            string destFilePath = Path.Combine(DirectoryManager.SettingsFiles_Directory, Path.GetFileName(filePath));

            string fileName = Path.GetFileNameWithoutExtension(filePath);

            File.Copy(filePath, destFilePath);

            return fileName;
        }

        /// <summary>
        /// Возвращает имена всех доступных файлов настроек.
        /// </summary>
        /// <returns></returns>
        public string[] FindFilesOfPresets()
        {
            string[] arrayOfPresets = DirectoryManager.CheckFiles(
                DirectoryManager.SettingsFiles_Directory, 
                FileName_DefaultPreset, 
                FileExtension, 
                DeviceData.GetDefault()
                );

            return arrayOfPresets;
        }

        /// <summary>
        /// Сохранение файла настроек приложения
        /// </summary>
        /// <param name="data"></param>
        public void SaveAppInfo(AppInfo data)
        {
            try
            {
                string filePath = Path.Combine(DirectoryManager.CommonFiles_Directory, FileName_AppData + FileExtension);

                FileIO.Save(filePath, data);

                AppData = data;
            }
            
            catch (Exception error)
            {
                throw new Exception("Ошибка сохранения настроек приложения.\n\n" + error.Message);
            }
        }

        /// <summary>
        /// Чтение из файла настроек приложения
        /// </summary>
        /// <returns></returns>
        private AppInfo ReadAppInfo()
        {
            string filePath = DirectoryManager.FindOrCreateFile(
                DirectoryManager.CommonFiles_Directory,
                FileName_AppData,
                FileExtension,
                AppInfo.GetDefault(FileName_DefaultPreset)
                );

            return FileIO.ReadOrCreateDefault(filePath, AppInfo.GetDefault(FileName_DefaultPreset));
        }

        /// <summary>
        /// Сохранение макросов
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="macros"></param>
        public void SaveMacros<T>(T macros)
        {
            string filePath = GetMacrosFilePath<T>(out _);

            FileIO.Save(filePath, macros);
        }

        /// <summary>
        /// Чтение макросов
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadMacros<T>()
        {
            string filePath = GetMacrosFilePath<T>(out object defaultMacrosValue);

            return FileIO.ReadOrCreateDefault(filePath, (T)defaultMacrosValue);
        }

        private string GetMacrosFilePath<T>(out object defaultMacrosValue)
        {
            string fileName;

            if (typeof(T) == typeof(MacrosModbus))
            {
                fileName = FileName_Macros_Modbus;
                defaultMacrosValue = new MacrosModbus();
            }

            else if (typeof(T) == typeof(MacrosNoProtocol))
            {
                fileName = FileName_Macros_NoProtocol;
                defaultMacrosValue = new MacrosNoProtocol();
            }

            else
            {
                throw new Exception("Попытка сохранить неподдерживаемый тип макросов.");
            }

            return DirectoryManager.FindOrCreateFile(
                DirectoryManager.Macros_Directory,
                fileName,
                FileExtension,
                defaultMacrosValue
                );
        }
    }
}
