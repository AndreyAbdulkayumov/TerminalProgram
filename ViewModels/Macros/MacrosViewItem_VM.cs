using MessageBox_Core;
using ReactiveUI;
using System.Reactive;

namespace ViewModels.Macros
{
    public class MacrosViewItem_VM : ReactiveObject
    {
        private string _title = string.Empty;

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public ReactiveCommand<Unit, Unit> Command_EditMacros { get; }
        public ReactiveCommand<Unit, Unit> Command_MacrosDelete { get; }

        private readonly Func<Task> _clickAction;
        private readonly IMessageBox _messageBox;

        public MacrosViewItem_VM(string title, Func<Task> clickAction, Func<string, Task> editMacros, Action<string> deleteMacrosAction, IMessageBox messageBox)
        {
            Title = title;

            _clickAction = clickAction;
            _messageBox = messageBox;

            Command_EditMacros = ReactiveCommand.CreateFromTask(() => editMacros(Title));
            Command_EditMacros.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка редактирования макроса \"{Title}\"", MessageType.Error));

            Command_MacrosDelete = ReactiveCommand.CreateFromTask(() => DeleteMacros(deleteMacrosAction));
            Command_MacrosDelete.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка удаления макроса \"{Title}\"", MessageType.Error));
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
