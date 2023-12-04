using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Models.Settings
{
    internal class AppDirectoryManager
    {
        private const string CommonFolderName = "XSoft";
        private const string ProgramFolderName = "TerminalProgram";

        public static readonly string SettingsFiles_Directory;

        private const string SettingsFiles_FolderName = "Settings";


        static AppDirectoryManager()
        {
            string FolderInDocuments = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                CommonFolderName,
                ProgramFolderName);

            SettingsFiles_Directory = Path.Combine(FolderInDocuments, SettingsFiles_FolderName);
        }

        public static string[] CheckFiles(string SelectedDirectory, string DefaultFileName, string Extension, object DefaultData)
        {
            if (Directory.Exists(SelectedDirectory) == false)
            {
                Directory.CreateDirectory(SelectedDirectory);
            }

            string[] ArrayOfFileNames = Directory.GetFiles(SelectedDirectory, "*" + Extension);

            JsonSerializerOptions Options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Создание файла настроек по умолчанию, если в папке нет ни одного файла.
            if (ArrayOfFileNames.Length == 0)
            {
                string DefaultFile_FullPath = Path.Combine(SelectedDirectory, DefaultFileName + Extension);

                File.Create(DefaultFile_FullPath).Close();

                using (FileStream stream = new FileStream(DefaultFile_FullPath, FileMode.OpenOrCreate))
                {
                    JsonSerializer.Serialize(stream, DefaultData, Options);
                }

                ArrayOfFileNames = Directory.GetFiles(SelectedDirectory, "*" + Extension);
            }

            for (int i = 0; i < ArrayOfFileNames.Length; i++)
            {
                ArrayOfFileNames[i] = Path.GetFileNameWithoutExtension(ArrayOfFileNames[i]);
            }

            return ArrayOfFileNames;
        }
    }
}
