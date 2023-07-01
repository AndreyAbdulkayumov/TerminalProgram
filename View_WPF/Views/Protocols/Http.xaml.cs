using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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
        private readonly ViewModel_Http ViewModel;

        public Http()
        {
            InitializeComponent();

            ViewModel = new ViewModel_Http(MessageView.Show);

            DataContext = ViewModel;
        }

        private async void Page_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        await ViewModel.Command_SendRequest.Execute();
                        break;
                }
            }

            catch (Exception)
            {
                // Ошибка обрабатывавется во ViewModel
            }
        }

        private void Button_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBlock_RX.Text == "")
                {
                    MessageView.Show("Поле приема не содержит данных.", MessageType.Warning);

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
                MessageView.Show("Ошибка при попытке сохранить данные поля приема в файл:\n\n" + error.Message, MessageType.Error);

                TextBox_TX.Focus();
            }
        }
    }
}
