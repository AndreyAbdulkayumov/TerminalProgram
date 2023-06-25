using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using View_WPF.ViewModels;

namespace View_WPF.Views
{
    internal static class MessageView
    {
        public static void Show(string Message, MessageType Type)
        {
            MessageBoxImage Image = GetViewContent(Type, out string Title);            

            MessageBox.Show(Message, Title, MessageBoxButton.OK, Image);
        }

        public static bool ShowDialog(string Message, MessageType Type)
        {
            MessageBoxImage Image = GetViewContent(Type, out string Title);

            if (MessageBox.Show(Message, Title, MessageBoxButton.YesNo, Image) == MessageBoxResult.Yes)
            {
                return true;
            }

            return false;
        }

        private static MessageBoxImage GetViewContent(MessageType Type, out string Title)
        {
            switch (Type)
            {
                case MessageType.Error:
                    Title = "Ошибка";
                    return MessageBoxImage.Error;

                case MessageType.Warning:
                    Title = "Предупреждение";
                    return MessageBoxImage.Warning;

                case MessageType.Information:
                    Title = "Сообщение";
                    return MessageBoxImage.Information;

                default:
                    Title = "Сообщение";
                    return MessageBoxImage.Information;
            }
        }
    }
}
