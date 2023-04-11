using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TerminalProgram.Protocols.NoProtocol
{
    public partial class UI_NoProtocol : Page
    {
        private object locker = new object();

        private void SendMessage(string StringMessage, bool CR_Enable, bool CF_Enable)
        {
            if (Client == null)
            {
                MessageBox.Show("Клиент не инициализирован.", MainWindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);

                return;
            }

            if (StringMessage == String.Empty)
            {
                MessageBox.Show("Буфер для отправления пуст. Введите в поле TX отправляемое значение.", MainWindowTitle,
                    MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);

                return;
            }

            List<byte> Message = new List<byte>(MainWindow.GlobalEncoding.GetBytes(StringMessage));

            if (CR_Enable == true)
            {
                Message.Add((byte)'\r');
            }

            if (CF_Enable == true)
            {
                Message.Add((byte)'\n');
            }

            Client.Send(Message.ToArray(), Message.Count);
        }

        private void Client_DataReceived(object? sender, DataFromDevice e)
        {
            lock (locker)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                    new Action(delegate
                    {
                        try
                        {
                            TextBox_RX.AppendText(MainWindow.GlobalEncoding.GetString(e.RX));

                            if (CheckBox_NextLine.IsChecked == true)
                            {
                                TextBox_RX.AppendText("\n");
                            }

                            TextBox_RX.LineDown();
                            ScrollViewer_RX.ScrollToEnd();
                        }

                        catch (Exception error)
                        {
                            if (ErrorHandler != null)
                            {
                                ErrorHandler(this, new EventArgs());

                                MessageBox.Show("Возникла ошибка при приеме данных:\n" + error.Message +
                                    "\n\nКлиент был отключен.", MainWindowTitle,
                                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                            }

                            else
                            {
                                MessageBox.Show("Возникла ошибка при приеме данных:\n" + error.Message +
                                    "\n\nКлиент не был отключен.", MainWindowTitle,
                                    MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                            }
                        }
                    }));
            }
        }
    }
}
