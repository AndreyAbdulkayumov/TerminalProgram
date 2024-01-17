using MessageBox_Core;
using MessageBox_WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace TerminalProgram.Views
{
    /// <summary>
    /// Логика взаимодействия для AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private readonly WPF_MessageView MessageView;


        public AboutWindow(WPF_MessageView MessageView)
        {
            InitializeComponent();

            char[] VersionChars = new char[20];

            if (Assembly.GetExecutingAssembly().GetName().Version?.TryFormat(VersionChars, 3, out int NumberOfChars) == true)
            {
                TextBlock_Version.Text = new string(VersionChars, 0, NumberOfChars);
            }            

            this.MessageView = MessageView;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Space || e.Key == Key.Enter)
            {
                Close();
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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c start {e.Uri.AbsoluteUri}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                }

                e.Handled = true;
            }

            catch (Exception error)
            {
                MessageView.Show("Ошибка перехода по ссылке.\n\n" + error.Message, MessageType.Error);
            }
        }
    }
}
