using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using TerminalProgram.ServiceWindows;

namespace TerminalProgram.Settings
{
    public partial class SettingsWindow : Window
    {
        private async void Button_File_AddNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EnterTextWindow window = new EnterTextWindow()
                {
                    Owner = this
                };

                window.ShowDialog();

                if (window.FileName != String.Empty)
                {
                    Settings = SystemOfSettings.GetDefault();

                    SystemOfSettings.Settings_FilePath =
                        UsedDirectories.GetPath(ProgramDirectory.Settings) +
                        window.FileName +
                        SystemOfSettings.FileType;

                    await SystemOfSettings.Save(Settings);

                    string[] Files = MainWindow.GetDeviceList();

                    ComboBoxFilling(ComboBox_SelectedDevice, ref Files);

                    ComboBox_SelectedDevice.SelectedValue = window.FileName;
                }
            }

            catch(Exception error)
            {
                MessageBox.Show("Ошибка при создании нового файла.\n\n" + error.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_File_AddExisting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog FileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Добавление уже существующего файла настроек подключения",
                    Filter = "Файл настроек|*.xml" // Filter files by extension
                };

                // Show open file dialog box
                Nullable<bool> result = FileDialog.ShowDialog();

                // Process open file dialog box results
                if (result == true)
                {
                    File.Copy(FileDialog.FileName, UsedDirectories.GetPath(ProgramDirectory.Settings) + FileDialog.SafeFileName);

                    string FileName = System.IO.Path.GetFileNameWithoutExtension(FileDialog.SafeFileName);

                    string[] Devices = MainWindow.GetDeviceList();
                    ComboBoxFilling(ComboBox_SelectedDevice, ref Devices);

                    ComboBox_SelectedDevice.SelectedValue = FileName;
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка при добавлении уже существующего файла.\n\n" + error.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_File_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBox_SelectedDevice.Items.Count <= 1)
                {
                    MessageBox.Show("Нельзя удалить единственный файл.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return;
                }

                string? SelectedFile = ComboBox_SelectedDevice.SelectedValue?.ToString();
                int DeletedIndex = ComboBox_SelectedDevice.SelectedIndex;

                if (MessageBox.Show("Вы действительно желайте удалить файл " + SelectedFile + "?", "Предупреждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (SystemOfSettings.Settings_FilePath == null)
                    {
                        throw new Exception("Не инициализирован путь к выбранному файлу.");
                    }

                    File.Delete(SystemOfSettings.Settings_FilePath);

                    string[] Devices = MainWindow.GetDeviceList();
                    ComboBoxFilling(ComboBox_SelectedDevice, ref Devices);

                    // Если удален первый в списке файл, то отображаем следующий файл.
                    if (DeletedIndex <= 0)
                    {
                        ComboBox_SelectedDevice.SelectedIndex = DeletedIndex;
                    }

                    // Если удален последний или любой другой файл в списке, то переходим к предыдущему файлу
                    else
                    {
                        ComboBox_SelectedDevice.SelectedIndex = DeletedIndex - 1;
                    }


                    MessageBox.Show("Файл " + SelectedFile + " успешно удален.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                else
                {
                    return;
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка при удалении файла.\n\n" + error.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Button_File_Save_Click(object sender, RoutedEventArgs e)
        {
            await SystemOfSettings.Save(Settings);

            string? SelectedFile = ComboBox_SelectedDevice.SelectedValue?.ToString();

            if (SelectedFile == null)
            {
                throw new Exception("Не удалось обработать выбранное имя файла.");
            }

            SettingsDocument = SelectedFile;

            SettingsIsChanged = true;

            await DisplaySettingsFile();

            MessageBox.Show("Настройки успешно сохранены!", "Сообщение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
