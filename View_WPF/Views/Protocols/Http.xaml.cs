using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using View_WPF.ViewModels;
using View_WPF.ViewModels.MainWindow;

namespace View_WPF.Views.Protocols
{
    /// <summary>
    /// Логика взаимодействия для Http.xaml
    /// </summary>
    public partial class Http : Page
    {
        private readonly string MainWindowTitle;

        public Http(MainWindow window)
        {
            InitializeComponent();

            MainWindowTitle = window.Title;

            DataContext = new ViewModel_Http(MessageView.Show);
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ((ViewModel_Http)DataContext).SendRequest_Command.Execute();
                    break;
            }
        }

        private void Button_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBlock_RX.Text == "")
                {
                    MessageBox.Show("Поле приема не содержит данных.", MainWindowTitle,
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    TextBox_TX.Focus();

                    return;
                }

                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = "HttpResponse", // Имя по умолчанию
                    DefaultExt = ".txt",       // Расширение файла по умолчанию
                    Filter = "Text Document|*.txt|Xml|*.xml|JSON|*.json" // Допустимые форматы файла
                };

                Nullable<bool> result = dialog.ShowDialog();

                if (result == true)
                {
                    using (FileStream Stream = new FileStream(dialog.FileName, FileMode.OpenOrCreate))
                    {
                        byte[] Data = Encoding.Default.GetBytes(TextBlock_RX.Text);
                        Stream.Write(Data, 0, Data.Length);
                    }
                }
            }

            catch (Exception error)
            {
                MessageBox.Show("Ошибка при попытке сохранить данные поля приема в файл:\n\n" + error.Message, MainWindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);

                TextBox_TX.Focus();
            }
        }
    }
}
