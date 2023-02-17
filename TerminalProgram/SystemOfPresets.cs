using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Windows;

namespace TerminalProgram
{
    public static class SystemOfPresets
    {
        public static void Remove(string NameOfPreset)
        {
            if (File.Exists(UsedDirectories.GetPath(ProgramDirectory.Settings) + NameOfPreset + SystemOfSettings.FileType))
            {
                File.Delete(UsedDirectories.GetPath(ProgramDirectory.Settings) + NameOfPreset + SystemOfSettings.FileType);
            }

            else
            {
                throw new Exception("Файл " + NameOfPreset + " не найден.\n");
            }
        }

        public static async Task<string[]> FindFilesOfPresets()
        {
            string[] ArrayOfPresets;

            try
            {
                ArrayOfPresets = Directory.GetFiles(UsedDirectories.GetPath(ProgramDirectory.Settings), "*" + SystemOfSettings.FileType);
            }
            
            catch(DirectoryNotFoundException)
            {
                throw new Exception("Не найдена папка " + UsedDirectories.GetPath(ProgramDirectory.Settings) +
                    "\n\nВозможно, вам стоит создать ее вручную в директории исполняемого файла.");
            }

            catch(Exception error)
            {
                throw new Exception(error.Message);
            }

            if (ArrayOfPresets.Length == 0)
            {
                MessageBox.Show("В директории " + UsedDirectories.GetPath(ProgramDirectory.Settings) +
                    " не найдено ни одного пресета. Будет создан пресет по умолчанию.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error,
                    MessageBoxResult.OK);

                string NewFilePath = UsedDirectories.GetPath(ProgramDirectory.Settings) + "Unknown" + SystemOfSettings.FileType;

                File.Create(NewFilePath).Close();

                SystemOfSettings.Settings_FilePath = NewFilePath;
                await SystemOfSettings.Save(SystemOfSettings.GetDefault());

                ArrayOfPresets = Directory.GetFiles(UsedDirectories.GetPath(ProgramDirectory.Settings), "*" + SystemOfSettings.FileType);
            }

            int Index;

            for (int i = 0; i < ArrayOfPresets.Length; i++)
            {
                // Вычленение имя файла и его расширения из полного пути
                Index = ArrayOfPresets[i].LastIndexOf('/');
                ArrayOfPresets[i] = ArrayOfPresets[i].Substring(Index + 1);

                // Получение имени файла без расширения
                Index = ArrayOfPresets[i].IndexOf('.');
                ArrayOfPresets[i] = ArrayOfPresets[i].Remove(Index);
            }

            return ArrayOfPresets;
        }
    }
}
