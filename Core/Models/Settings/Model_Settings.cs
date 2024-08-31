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
        public void Delete(string fileName)
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
        /// Копирует файл с указанным путем в директорию приложения.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>Имя скопированного файла без расширения.</returns>
        public string CopyFrom(string filePath)
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
    }
}
