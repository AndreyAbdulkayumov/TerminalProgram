using System.Text.Json;

namespace Core.Models.Settings
{
    internal class AppDirectoryManager
    {
        // Имя папки приложения
        private const string ProgramFolderName = "TerminalProgram";

        /*******************************************************/
        //
        // Константы с определением имен папок приложения
        //
        /*******************************************************/

        // Папка с файлами настроек
        private const string SettingsFiles_FolderName = "Settings";

        // Папка с файлами логов
        private const string LogFiles_FolderName = "LogFiles";

        // Папка с общими файлами (сейчас общие файлы лежат в корне папки приложения)
        private readonly string CommonFiles_FolderName = string.Empty;

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


        public AppDirectoryManager()
        {
            string FolderInDocuments = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal), ProgramFolderName);

            // Создание путей к папкам приложения

            SettingsFiles_Directory = Path.Combine(FolderInDocuments, SettingsFiles_FolderName);

            LogFiles_Directory = Path.Combine(FolderInDocuments, LogFiles_FolderName);

            CommonFiles_Directory = Path.Combine(FolderInDocuments, CommonFiles_FolderName);
        }

        /// <summary>
        /// Проверка наличия файлов в указанной директории. Если директория пуста, то создается один файл с заданными данными по умолчанию.
        /// </summary>
        /// <param name="SelectedDirectory"></param>
        /// <param name="DefaultFileName"></param>
        /// <param name="Extension"></param>
        /// <param name="DefaultData"></param>
        /// <returns>Возвращает массив имен файлов в заданной директории.</returns>
        public string[] CheckFiles(string SelectedDirectory, string DefaultFileName, string Extension, object DefaultData)
        {
            if (Directory.Exists(SelectedDirectory) == false)
            {
                Directory.CreateDirectory(SelectedDirectory);
            }

            string[] ArrayOfFileNames = Directory.GetFiles(SelectedDirectory, "*" + Extension);           

            // Создание файла по умолчанию, если в папке нет ни одного файла.
            if (ArrayOfFileNames.Length == 0)
            {
                CreateDefaultFile(SelectedDirectory, DefaultFileName, Extension, DefaultData);

                ArrayOfFileNames = Directory.GetFiles(SelectedDirectory, "*" + Extension);
            }

            for (int i = 0; i < ArrayOfFileNames.Length; i++)
            {
                ArrayOfFileNames[i] = Path.GetFileNameWithoutExtension(ArrayOfFileNames[i]);
            }

            return ArrayOfFileNames;
        }

        /// <summary>
        /// Поиск файла по заданному пути. Если файл не найден, то будет создан новый файл с заданными данными по умолчанию.
        /// </summary>
        /// <param name="SelectedDirectory"></param>
        /// <param name="SelectedFileName"></param>
        /// <param name="Extension"></param>
        /// <param name="DefaultData"></param>
        /// <returns>Возвращает полный путь к файлу.</returns>
        public string FindOrCreateFile(string SelectedDirectory, string FileName, string Extension, object DefaultData)
        {
            if (Directory.Exists(SelectedDirectory) == false)
            {
                Directory.CreateDirectory(SelectedDirectory);
            }

            string[] ArrayOfFileNames = Directory.GetFiles(SelectedDirectory, FileName + Extension);

            // Создание файла по умолчанию, если в папке нет ни одного файла.
            if (ArrayOfFileNames.Length == 0)
            {
                CreateDefaultFile(SelectedDirectory, FileName, Extension, DefaultData);

                ArrayOfFileNames = Directory.GetFiles(SelectedDirectory, FileName + Extension);
            }

            return ArrayOfFileNames.First();
        }

        private void CreateDefaultFile(string SelectedDirectory, string DefaultFileName, string Extension, object DefaultData)
        {
            string DefaultFile_FullPath = Path.Combine(SelectedDirectory, DefaultFileName + Extension);

            File.Create(DefaultFile_FullPath).Close();

            JsonSerializerOptions Options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            using (FileStream stream = new FileStream(DefaultFile_FullPath, FileMode.Open))
            {
                JsonSerializer.Serialize(stream, DefaultData, Options);
            }
        }
    }
}
