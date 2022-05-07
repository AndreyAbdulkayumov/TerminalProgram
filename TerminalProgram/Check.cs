using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace TerminalProgram
{
    internal static class Check
    {
        public static void FilesDirectory()
        {
            try
            {
                if (Directory.Exists(UsedDirectories.GetPath(ProgramDirectory.Settings)) == false)
                {
                    MessageBox.Show("Не найдена директория для хранения пресетов ( " +
                        UsedDirectories.GetPath(ProgramDirectory.Settings) + " ).\n\n" +
                        "Данная директория будет создана рядом с исполняемым файлом.\n\n" +
                        "Нажмите ОК для продолжения.",
                        "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error, MessageBoxResult.OK);

                    Directory.CreateDirectory(UsedDirectories.GetPath(ProgramDirectory.Settings));
                }
            }
            catch (Exception error)
            {
                throw new Exception("Не удалось создать папку для хранения пресетов.\n\n" + error.Message);
            }
        }

        public static bool SettingsFile(ref string[] PresetFileNames, string SettingsFileName)
        {
            try
            {
                if (SettingsFileName == String.Empty)
                {
                    return false;
                }

                bool FileIsExisting = false;

                foreach (string Path in PresetFileNames)
                {
                    if (Path == SettingsFileName)
                    {
                        FileIsExisting = true;
                        break;
                    }
                }

                if (FileIsExisting == false)
                {
                    return false;
                }

                return true;
            }

            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
    }
}
