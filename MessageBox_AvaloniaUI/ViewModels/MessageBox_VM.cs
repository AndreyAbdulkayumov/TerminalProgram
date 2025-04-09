using MessageBox_AvaloniaUI.Views;
using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace MessageBox_AvaloniaUI.ViewModels
{
    public class MessageBox_VM : ReactiveObject
    {
        private string? _title;

        public string? Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        private string? _content;

        public string? Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        private MessageType _type = MessageType.Information;

        public MessageType Type
        {
            get => _type;
            set => this.RaiseAndSetIfChanged(ref _type, value); 
        }

        public ObservableCollection<ButtonContent> Buttons { get; set; } = new ObservableCollection<ButtonContent>();

        public const string Content_OK = "ОК";
        public const string Content_Yes = "Да";
        public const string Content_No = "Нет";


        public MessageBox_VM(string message, string title, MessageType messageType, MessageBoxToolType toolType)
        {
            Content = message;
            Title = title;

            Type = messageType;

            switch (toolType)
            {
                case MessageBoxToolType.Default:
                    Buttons.Add(new ButtonContent(Content_OK));
                    break;

                case MessageBoxToolType.YesNo:
                    Buttons.Add(new ButtonContent(Content_Yes));
                    Buttons.Add(new ButtonContent(Content_No));
                    break;

                default:
                    Buttons.Add(new ButtonContent(Content_OK));
                    break;
            }
        }
    }
}
