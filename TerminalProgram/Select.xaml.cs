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

namespace TerminalProgram
{
    /// <summary>
    /// Логика взаимодействия для Select.xaml
    /// </summary>
    public partial class Select : Window
    {
        public string SelectedDocumentPath { get; private set; } = String.Empty;

        private readonly string[] ArrayOfDocuments;


        public Select(ref string[] ArrayOfDocuments)
        {
            InitializeComponent();

            this.ArrayOfDocuments = ArrayOfDocuments;

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
                    "Попробуйте вручную создать папку Settings и добавить туда .xml файл с настройками.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                Close();
            }
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SelectedDocumentPath == String.Empty)
            {
                if (MessageBox.Show("Документ не выбран. Вы действительно хотите выйти?", "Предупреждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void Button_Select_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Документ \"" + ComboBox_SelectedDocument.SelectedItem.ToString() + "\" успешно выбран.", "Сообщение",
                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);

            SelectedDocumentPath = ComboBox_SelectedDocument.SelectedItem.ToString();

            Close();
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
    }
}
