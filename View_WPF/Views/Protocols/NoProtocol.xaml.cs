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
    public enum TypeOfMessage
    {
        Char,
        String
    };

    /// <summary>
    /// Логика взаимодействия для NoProtocol.xaml
    /// </summary>
    public partial class NoProtocol : Page
    {
        private readonly ViewModel_NoProtocol ViewModel;

        public NoProtocol()
        {
            InitializeComponent();

            ViewModel = new ViewModel_NoProtocol(
                MessageView.Show,
                SetUI_Connected, 
                SetUI_Disconnected,
                Action_Receive,
                Action_Clear_ReceiveField
                );

            DataContext = ViewModel;
        }

        private void SetUI_Connected()
        {
            TextBox_TX.IsEnabled = true;

            CheckBox_CR.IsEnabled = true;
            CheckBox_LF.IsEnabled = true;
            RadioButton_Char.IsEnabled = true;
            RadioButton_String.IsEnabled = true;

            if (RadioButton_String.IsChecked == true)
            {
                Button_Send.IsEnabled = true;
            }

            else
            {
                Button_Send.IsEnabled = false;
            }

            TextBox_TX.Focus();
        }

        private void SetUI_Disconnected()
        {
            TextBox_TX.IsEnabled = false;

            CheckBox_CR.IsEnabled = false;
            CheckBox_LF.IsEnabled = false;
            RadioButton_Char.IsEnabled = false;
            RadioButton_String.IsEnabled = false;
            Button_Send.IsEnabled = false;
        }

        private void Action_Receive(string Data)
        {
            // Ради повышения производительности
            // в этом месте пришлось отказаться от конструкции try - catch

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                    new Action(delegate
                    {
                        TextBox_RX.AppendText(Data);
                        TextBox_RX.LineDown();
                        ScrollViewer_RX.ScrollToEnd();
                    }));
        }

        private void Action_Clear_ReceiveField()
        {
            TextBox_RX.Clear();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButton_String.Checked -= RadioButton_String_Checked;
            RadioButton_String.IsChecked = true;
            RadioButton_String.Checked += RadioButton_String_Checked;

            TextBox_TX.Text = String.Empty;

            TextBox_TX.Focus();
        }

        private async void Page_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Enter:
                        await ViewModel.Command_Send.Execute();
                        break;
                }
            }

            catch (Exception)
            {
                // Ошибка обрабатывавется во ViewModel
            }
        }

        private void CheckBox_CR_Click(object sender, RoutedEventArgs e)
        {
            TextBox_TX.Focus();
        }

        private void CheckBox_LF_Click(object sender, RoutedEventArgs e)
        {
            TextBox_TX.Focus();
        }

        private void RadioButton_Char_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_TX.Focus();
        }

        private void RadioButton_String_Checked(object sender, RoutedEventArgs e)
        {
            TextBox_TX.Focus();
        }

        private void Button_SaveAs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBox_RX.Text == String.Empty)
                {
                    MessageView.Show("Поле приема не содержит данных.", MessageType.Warning);

                    TextBox_TX.Focus();

                    return;
                }

                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = "HostResponse", // Имя по умолчанию
                    DefaultExt = ".txt",       // Расширение файла по умолчанию
                    Filter = "Text Document|*.txt" // Допустимые форматы файла
                };

                Nullable<bool> result = dialog.ShowDialog();

                if (result == true)
                {
                    using (FileStream Stream = new FileStream(dialog.FileName, FileMode.OpenOrCreate))
                    {
                        byte[] Data = Encoding.UTF8.GetBytes(TextBox_RX.Text);
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
