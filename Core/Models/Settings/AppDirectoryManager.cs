using System.Text.Json;

namespace Core.Models.Settings
{
    internal class AppDirectoryManager
    {
        /// <summary>
        /// Имя папки приложения
        /// </summary>
        private const string ProgramFolderName = "TerminalProgram";

        /*******************************************************/
        //
        // Константы с определением имен папок приложения
        //
        /*******************************************************/

        /// <summary>
        /// Папка с файлами настроек
        /// </summary>
        private const string SettingsFiles_FolderName = "Settings";

        /// <summary>
        /// Папка с файлами логов
        /// </summary>
        private const string LogFiles_FolderName = "LogFiles";

        /// <summary>
        /// Папка с общими файлами (сейчас общие файлы лежат в корне папки приложения)
        /// </summary>
        private readonly string CommonFiles_FolderName = string.Empty;

        /// <summary>
        /// Папка с файлами макросов
        /// </summary>
        private const string Macros_FolderName = "Macros";

        /*******************************************************/
        //
        // Полные пути к папкам приложения
        //
        /*******************************************************/

        /// <summary>
        /// Полный путь к папке настроек
        /// </summary>
        public readonly string SettingsFiles_Directory;

        /// <summary>
        /// Полный путь к папке файлов с логами
        /// </summary>
        public readonly string LogFiles_Directory;

        /// <summary>
        /// Полный путь к папке с общими файлами
        /// </summary>
        public readonly string CommonFiles_Directory;

        /// <summary>
        /// Полный путь к папке макросов
        /// </summary>
        public readonly string Macros_Directory;


        public AppDirectoryManager()
        {
            string folderInDocuments = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal), ProgramFolderName);

            // Создание путей к папкам приложения

            SettingsFiles_Directory = Path.Combine(folderInDocuments, SettingsFiles_FolderName);

            LogFiles_Directory = Path.Combine(folderInDocuments, LogFiles_FolderName);

            CommonFiles_Directory = Path.Combine(folderInDocuments, CommonFiles_FolderName);

            Macros_Directory = Path.Combine(folderInDocuments, Macros_FolderName);
        }

        /// <summary>
        /// Проверка наличия файлов в указанной директории. Если директория пуста, то создается один файл с заданными данными по умолчанию.
        /// </summary>
        /// <param name="selectedDirectory"></param>
        /// <param name="defaultFileName"></param>
        /// <param name="extension"></param>
        /// <param name="defaultData"></param>
        /// <returns>Возвращает массив имен файлов в заданной директории.</returns>
        public string[] CheckFiles(string selectedDirectory, string defaultFileName, string extension, object defaultData)
        {
            if (Directory.Exists(selectedDirectory) == false)
            {
                Directory.CreateDirectory(selectedDirectory);
            }

            string[] arrayOfFileNames = Directory.GetFiles(selectedDirectory, "*" + extension);           

            // Создание файла по умолчанию, если в папке нет ни одного файла.
            if (arrayOfFileNames.Length == 0)
            {
                CreateDefaultFile(selectedDirectory, defaultFileName, extension, defaultData);

                arrayOfFileNames = Directory.GetFiles(selectedDirectory, "*" + extension);
            }

            for (int i = 0; i < arrayOfFileNames.Length; i++)
            {
                arrayOfFileNames[i] = Path.GetFileNameWithoutExtension(arrayOfFileNames[i]);
            }

            return arrayOfFileNames;
        }

        /// <summary>
        /// Поиск файла по заданному пути. Если файл не найден, то будет создан новый файл с заданными данными по умолчанию.
        /// </summary>
        /// <param name="selectedDirectory"></param>
        /// <param name="fileName"></param>
        /// <param name="extension"></param>
        /// <param name="defaultData"></param>
        /// <returns>Возвращает полный путь к файлу.</returns>
        public string FindOrCreateFile(string selectedDirectory, string fileName, string extension, object defaultData)
        {
            if (Directory.Exists(selectedDirectory) == false)
            {
                Directory.CreateDirectory(selectedDirectory);
            }

            string[] arrayOfFileNames = Directory.GetFiles(selectedDirectory, fileName + extension);

            // Создание файла по умолчанию, если в папке нет ни одного файла.
            if (arrayOfFileNames.Length == 0)
            {
                CreateDefaultFile(selectedDirectory, fileName, extension, defaultData);

                arrayOfFileNames = Directory.GetFiles(selectedDirectory, fileName + extension);
            }

            return arrayOfFileNames.First();
        }

        private void CreateDefaultFile(string selectedDirectory, string defaultFileName, string extension, object defaultData)
        {
            string defaultFile_FullPath = Path.Combine(selectedDirectory, defaultFileName + extension);

            File.Create(defaultFile_FullPath).Close();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            using (var stream = new FileStream(defaultFile_FullPath, FileMode.Open))
            {
                JsonSerializer.Serialize(stream, defaultData, options);
            }
        }
    }
}
