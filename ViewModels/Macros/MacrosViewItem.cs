using MessageBox_Core;
using ReactiveUI;
using System.Reactive;

namespace ViewModels.Macros
{
    public class MacrosViewItem : ReactiveObject
    {
        public string Title { get; set; }
        public ReactiveCommand<Unit, Unit> Command_ItemAction { get; set; }

        public MacrosViewItem(string title, Func<Task> clickAction, IMessageBox _messageBox)
        {
            Title = title;

            Command_ItemAction = ReactiveCommand.CreateFromTask(clickAction);
            Command_ItemAction.ThrownExceptions.Subscribe(error => _messageBox.Show(error.Message, MessageType.Error));
        }
    }
}
