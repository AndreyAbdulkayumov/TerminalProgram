using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace TerminalProgram.Protocols.Http
{
    /// <summary>
    /// Логика взаимодействия для UI_Http.xaml
    /// </summary>
    public partial class UI_Http : Page
    {
        private readonly string MainWindowTitle;

        public UI_Http(MainWindow window)
        {
            InitializeComponent();

            MainWindowTitle = window.Title;
        }

        private async void Button_Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();

                List<Task> Tasks = new List<Task>();

                Task<HttpResponseMessage> HttpRequest = client.GetAsync(TextBox_TX.Text);
                Tasks.Add(HttpRequest);

                HttpResponseMessage Response = HttpRequest.Result;
                Response.EnsureSuccessStatusCode();

                Task<string> DecodedResponse = Response.Content.ReadAsStringAsync();
                Tasks.Add(DecodedResponse);

                await Task.WhenAll(Tasks);

                TextBlock_RX.Text = DecodedResponse.Result;
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Ошибка отправки http запроса:\n\n" + error.Message + "\n\nУказанный URI:\n\n" + TextBox_TX.Text,
                    MainWindowTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                TextBox_TX.Focus();
            }
        }

        private void Button_ClearFieldRX_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_RX.Text = String.Empty;

            TextBox_TX.Focus();
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    Button_Send_Click(Button_Send, new RoutedEventArgs());
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
                    FileName = "HttpResponse", // Default file name
                    DefaultExt = ".txt", // Default file extension
                    Filter = "Text documents (.txt)|*.txt" // Filter files by extension
                };

                Nullable<bool> result = dialog.ShowDialog();

                if (result == true)
                {
                    using (FileStream Stream = new FileStream(dialog.FileName, FileMode.OpenOrCreate))
                    {
                        byte[] Data = Encoding.UTF8.GetBytes(TextBlock_RX.Text);
                        Stream.Write(Data, 0, Data.Length);
                    }
                }
            }
            
            catch(Exception error)
            {
                MessageBox.Show("Ошибка при попытке сохранить данные поля приема в файл:\n\n" + error.Message, MainWindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);

                TextBox_TX.Focus();
            }
        }
    }
}
