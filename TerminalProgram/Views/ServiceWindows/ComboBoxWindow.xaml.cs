using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TerminalProgram.Views.ServiceWindows
{
    /// <summary>
    /// Логика взаимодействия для ComboBoxWindow.xaml
    /// </summary>
    public partial class ComboBoxWindow : Window
    {
        public string SelectedDocumentPath { get; private set; } = String.Empty;


        public ComboBoxWindow(string[] ArrayOfDocuments)
        {
            InitializeComponent();

            for (int i = 0; i < ArrayOfDocuments.Length; i++)
            {
                ComboBox_SelectedDocument.Items.Add(ArrayOfDocuments[i]);
            }

            if (ComboBox_SelectedDocument.Items.Count > 0)
            {
                ComboBox_SelectedDocument.SelectedIndex = 0;
            }
            
            else
            {
                MessageBox.Show("Список пуст.\n" +
                    "Закройте это окно.\n" +
                    "Попробуйте вручную создать папку Settings и добавить туда файл с настройками.", this.Title, 
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SelectedDocumentPath == String.Empty)
            {
                if (MessageBox.Show("Документ не выбран. Вы действительно хотите выйти?", this.Title,
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Button_Select_Click(Button_Select, new RoutedEventArgs());
                    break;

                case Key.Escape:
                    Close();
                    break;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            string? SelectedFile = ComboBox_SelectedDocument.SelectedItem?.ToString();

            if (SelectedFile == null)
            {
                SelectedDocumentPath = String.Empty;

                MessageBox.Show("Не удалось выбрать документ.", this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }

            else
            {
                SelectedDocumentPath = SelectedFile;

                MessageBox.Show("Документ \"" + ComboBox_SelectedDocument.SelectedItem?.ToString() +
                    "\" успешно выбран.", this.Title,
                    MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }

            Close();
        }
    }
}
