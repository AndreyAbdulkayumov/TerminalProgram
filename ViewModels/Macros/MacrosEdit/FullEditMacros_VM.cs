using DynamicData;
using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Runtime.Intrinsics.Arm;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros.MacrosEdit
{
    public class FullEditMacros_VM : ReactiveObject
    {
        private string? _macrosName = string.Empty;

        public string? MacrosName
        {
            get => _macrosName;
            set => this.RaiseAndSetIfChanged(ref _macrosName, value);
        }

        private ObservableCollection<MacrosCommandItem_VM> _commandItems = new ObservableCollection<MacrosCommandItem_VM>();

        public ObservableCollection<MacrosCommandItem_VM> CommandItems
        {
            get => _commandItems;
            set => this.RaiseAndSetIfChanged(ref _commandItems, value);
        }

        public ReactiveCommand<Unit, Unit> Command_SaveMacros { get; }
        public ReactiveCommand<Unit, Unit> Command_AddCommand { get; }
        public ReactiveCommand<Unit, Unit> Command_AddDelay { get; }

        public FullEditMacros_VM(IMessageBox messageBox)
        {
            Command_SaveMacros = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrWhiteSpace(MacrosName))
                {
                    messageBox.Show("Задайте имя макроса.", MessageType.Warning);
                    return;
                }

                //Saved = true;
                //closeWindowAction();
            });
            Command_SaveMacros.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка сохранения макроса.\n\n{error.Message}", MessageType.Error));

            Command_AddCommand = ReactiveCommand.Create(() =>
            {
                CommandItems.Add(new MacrosCommandItem_VM(null, (CommandItems.Count() + 1).ToString(), RemoveCommand, messageBox));
            });
            Command_AddCommand.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка добавления команды.\n\n{error.Message}", MessageType.Error));

            Command_AddDelay = ReactiveCommand.Create(() => { });
            Command_AddDelay.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка добавления задержки.\n\n{error.Message}", MessageType.Error));
        }

        private void RemoveCommand(Guid selectedId)
        {
            var newCollection = CommandItems
                .Where(e => e.Id != selectedId)
                .ToList();

            CommandItems.Clear();
            CommandItems.AddRange(newCollection);
        }
    }
}
