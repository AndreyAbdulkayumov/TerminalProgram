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
        public static void CreateNew(string NameOfPreset)
        {
            XmlDocument Preset = new XmlDocument();

            Preset.LoadXml("<Settings>" + "</Settings>");

            XmlDeclaration PresetDeclaration = Preset.CreateXmlDeclaration("1.0", "utf-8", null);

            XmlElement Root = Preset.DocumentElement;
            Preset.InsertBefore(PresetDeclaration, Root);

            Preset.Save(UsedDirectories.GetPath(ProgramDirectory.Settings) + NameOfPreset + ".xml");
        }


        public static void Remove(string NameOfPreset)
        {
            if (File.Exists(UsedDirectories.GetPath(ProgramDirectory.Settings) + NameOfPreset + ".xml"))
            {
                File.Delete(UsedDirectories.GetPath(ProgramDirectory.Settings) + NameOfPreset + ".xml");
            }

            else
            {
                throw new Exception("Файл " + NameOfPreset + " не найден.\n");
            }
        }

        public static void FindFilesOfPresets(ref string[] ArrayOfPresets)
        {
            try
            {
                ArrayOfPresets = Directory.GetFiles(UsedDirectories.GetPath(ProgramDirectory.Settings), "*.xml");
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

                CreateNew("Default");

                ArrayOfPresets = Directory.GetFiles(UsedDirectories.GetPath(ProgramDirectory.Settings), "*.xml");
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
        }
    }
}
