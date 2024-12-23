using MessageBox_Core;
using ReactiveUI;
using System.Reactive;

namespace ViewModels.Macros
{
    public class MacrosViewItem : ReactiveObject
    {
        public string Title { get; set; }

        public ReactiveCommand<Unit, Unit> Command_MacrosDelete { get; }

        private readonly Func<Task> _clickAction;
        private readonly IMessageBox _messageBox;

        public MacrosViewItem(string title, Func<Task> clickAction, Action<string> deleteMacrosAction, IMessageBox messageBox)
        {
            Title = title;

            _clickAction = clickAction;
            _messageBox = messageBox;

            Command_MacrosDelete = ReactiveCommand.CreateFromTask(() => DeleteMacros(deleteMacrosAction));
        }

        public async Task MacrosAction()
        {
            try
            {
                await _clickAction();
            }

            catch (Exception error)
            {
                _messageBox.Show(error.Message, MessageType.Error);
            }
        }

        private async Task DeleteMacros(Action<string> deleteMacrosAction)
        {
            if (await _messageBox.ShowYesNoDialog($"Вы действительно хотите удалить макрос \"{Title}\"?", MessageType.Information) == MessageBoxResult.Yes)
            {
                deleteMacrosAction(Title);
            }
        }
    }
}
