using ReactiveUI;
using System.Reactive;
using MessageBox.Core;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros.MacrosEdit;

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

    public readonly Guid Id;

    public MacrosCommandItem_VM(Guid id, EditCommandParameters parameters, Action<Guid> runCommandHandler, Action<Guid> editCommandHandler, Action<Guid> removeItemHandler, IMessageBox messageBox)
    {
        Id = id;

        CommandName = parameters.CommandName;

        Command_RunCommand = ReactiveCommand.Create(() => runCommandHandler(id));
        Command_RunCommand.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка запуска команды \"{CommandName}\".\n\n{error.Message}", MessageType.Error, error));

        Command_EditCommand = ReactiveCommand.Create(() => editCommandHandler(Id));
        Command_EditCommand.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка редактирования команды \"{CommandName}\".\n\n{error.Message}", MessageType.Error, error));

        Command_RemoveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (await messageBox.ShowYesNoDialog($"Вы действительно хотите удалить команду \"{CommandName}\"?", MessageType.Warning) == MessageBoxResult.Yes)
            {
                removeItemHandler(Id);
            }
        });
        Command_RemoveCommand.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка удаления команды \"{CommandName}\".\n\n{error.Message}", MessageType.Error, error));
    }
}
