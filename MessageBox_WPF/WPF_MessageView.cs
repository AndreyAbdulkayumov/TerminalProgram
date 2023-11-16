using MessageBox_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MessageBox_WPF
{
    public class WPF_MessageView : IMessageBox
    {
        public string Title = String.Empty;

        public WPF_MessageView(string Title)
        {
            this.Title = Title;
        }

        public void Show(string Message, MessageType Type)
        {
            MessageBoxImage Image = GetViewContent(Type);

            MessageBox.Show(Message, Title, MessageBoxButton.OK, Image);
        }

        public bool ShowDialog(string Message, MessageType Type)
        {
            MessageBoxImage Image = GetViewContent(Type);

            if (MessageBox.Show(Message, Title, MessageBoxButton.YesNo, Image) == MessageBoxResult.Yes)
            {
                return true;
            }

            return false;
        }

        private MessageBoxImage GetViewContent(MessageType Type)
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
