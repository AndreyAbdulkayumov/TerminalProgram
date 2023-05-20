using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using View_WPF.Views.ServiceWindows;

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
                    

                    string FileName = System.IO.Path.GetFileNameWithoutExtension(FileDialog.SafeFileName);

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
               
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка при удалении файла.\n\n" + error.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Button_File_Save_Click(object sender, RoutedEventArgs e)
        {
            

            MessageBox.Show("Настройки успешно сохранены!", "Сообщение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
