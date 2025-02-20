using Core.Models.Settings.FileTypes;
using MessageBox_Core;
using ReactiveUI;
using System.Reactive;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros.MacrosEdit
{
    public class MacrosCommandItem_VM : ReactiveObject
    {
        private string? _commandName = string.Empty;

        public string? CommandName
        {
            get => _commandName;
            set => this.RaiseAndSetIfChanged(ref _commandName, value);
        }

        public ReactiveCommand<Unit, Unit> Command_RunCommand { get; }
        public ReactiveCommand<Unit, Unit> Command_EditCommand { get; }
        public ReactiveCommand<Unit, Unit> Command_RemoveCommand { get; }

        public object? CommandData { get; private set; }

        public readonly Guid Id;

        public MacrosCommandItem_VM(EditCommandParameters parameters, Func<EditCommandParameters, Task<object?>> openEditCommandWindow, Action<Guid> removeItemHandler, IMessageBox messageBox)
        {
            Id = Guid.NewGuid();

            CommandData = parameters.InitData;

            CommandName = parameters.CommandName;

            Command_RunCommand = ReactiveCommand.Create(() => { });
            Command_RunCommand.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка запуска команды \"{CommandName}\".\n\n{error.Message}", MessageType.Error));

            Command_EditCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                CommandData = await openEditCommandWindow(parameters);

                if (CommandData is MacrosCommandModbus data)
                {
                    CommandName = data.Name;
                }                
            });
            Command_EditCommand.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка редактирования команды \"{CommandName}\".\n\n{error.Message}", MessageType.Error));

            Command_RemoveCommand = ReactiveCommand.CreateFromTask(async () => 
            { 
                if (await messageBox.ShowYesNoDialog($"Вы действительно хотите удалить команду \"{CommandName}\"?", MessageType.Warning) == MessageBoxResult.Yes)
                {
                    removeItemHandler(Id);
                }
            });
            Command_RemoveCommand.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка удаления команды \"{CommandName}\".\n\n{error.Message}", MessageType.Error));
        }
    }
}
