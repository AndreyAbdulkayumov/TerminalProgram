using ReactiveUI;
using System.Reactive;
using MessageBox_Core;
using Core.Models.Settings.DataTypes;
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

        private bool _isEdit;

        public bool IsEdit
        {
            get => _isEdit;
            set => this.RaiseAndSetIfChanged(ref _isEdit, value);
        }

        public ReactiveCommand<Unit, Unit> Command_RunCommand { get; }
        public ReactiveCommand<Unit, Unit> Command_EditCommand { get; }
        public ReactiveCommand<Unit, Unit> Command_RemoveCommand { get; }

        public object? CommandData { get; private set; }

        public readonly Guid Id;

        private EditCommandParameters _parameters;

        public MacrosCommandItem_VM(EditCommandParameters parameters, Action<Guid> editCommandHandler, Action<Guid> removeItemHandler, IMessageBox messageBox)
        {
            Id = Guid.NewGuid();

            CommandData = parameters.InitData;

            CommandName = parameters.CommandName;

            _parameters = parameters;

            Command_RunCommand = ReactiveCommand.Create(() => { });
            Command_RunCommand.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка запуска команды \"{CommandName}\".\n\n{error.Message}", MessageType.Error));

            Command_EditCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (CommandData != null)
                {
                    _parameters = new EditCommandParameters(_parameters.CommandName, CommandData, _parameters.ExistingCommandNames);
                }

                IsEdit = true;

                //CommandData = await openEditCommandWindow(_parameters);

                editCommandHandler(Id);

                if (CommandData is IMacrosCommand data)
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
