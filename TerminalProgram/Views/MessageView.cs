using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TerminalProgram.ViewModels;

namespace TerminalProgram.Views
{
    public enum MessageType
    {
        Error,
        Warning,
        Information
    }

    internal static class MessageView
    {
        // Должен быть указан при запуске программы
        public static string Title = String.Empty;

        public static void Show(string Message, MessageType Type)
        {
            MessageBoxImage Image = GetViewContent(Type);            

            MessageBox.Show(Message, Title, MessageBoxButton.OK, Image);
        }

        public static bool ShowDialog(string Message, MessageType Type)
        {
            MessageBoxImage Image = GetViewContent(Type);

            if (MessageBox.Show(Message, Title, MessageBoxButton.YesNo, Image) == MessageBoxResult.Yes)
            {
                return true;
            }

            return false;
        }

        private static MessageBoxImage GetViewContent(MessageType Type)
        {
            switch (Type)
            {
                case MessageType.Error:
                    return MessageBoxImage.Error;

                case MessageType.Warning:
                    return MessageBoxImage.Warning;

                case MessageType.Information:
                    return MessageBoxImage.Information;

                default:
                    return MessageBoxImage.Information;
            }
        }
    }
}
