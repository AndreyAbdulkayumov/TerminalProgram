using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;
using DynamicData;
using MessageBox_Core;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using ViewModels.Macros.DataTypes;

namespace ViewModels.Macros.MacrosEdit
{
    public class EditMacros_VM : ReactiveObject
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
        public ReactiveCommand<Unit, Unit> Command_RunMacros { get; }
        public ReactiveCommand<Unit, Unit> Command_AddCommand { get; }
        public ReactiveCommand<Unit, Unit> Command_AddDelay { get; }

        public bool Saved { get; private set; } = false;

        private readonly List<EditCommandParameters> _allCommandParameters = new List<EditCommandParameters>();

        public EditMacros_VM(List<EditCommandParameters>? allCommandParameters, Func<EditCommandParameters, Task<object?>> openEditCommandWindow, Action closeWindowAction, IMessageBox messageBox)
        {
            if (allCommandParameters != null)
            {
                _allCommandParameters.AddRange(allCommandParameters);
            }            

            Command_SaveMacros = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrWhiteSpace(MacrosName))
                {
                    messageBox.Show("Задайте имя макроса.", MessageType.Warning);
                    return;
                }

                Saved = true;
                closeWindowAction();
            });
            Command_SaveMacros.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка сохранения макроса.\n\n{error.Message}", MessageType.Error));

            Command_RunMacros = ReactiveCommand.Create(() => { });
            Command_RunMacros.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка запуска макроса.\n\n{error.Message}", MessageType.Error));

            Command_AddCommand = ReactiveCommand.Create(() =>
            {
                var _allCommandNames = CommandItems.Select(e => e.CommandName);

                _allCommandParameters.Add(new EditCommandParameters((CommandItems.Count() + 1).ToString(), null, _allCommandNames));

                CommandItems.Add(new MacrosCommandItem_VM(_allCommandParameters.Last(), openEditCommandWindow, RemoveCommand, messageBox)); 
            });
            Command_AddCommand.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка добавления команды.\n\n{error.Message}", MessageType.Error));

            Command_AddDelay = ReactiveCommand.Create(() => { });
            Command_AddDelay.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка добавления задержки.\n\n{error.Message}", MessageType.Error));

            InitUI();
        }

        public void InitUI()
        {

        }

        public object GetMacrosContent()
        {
            var currentMode = CommonUI_VM.CurrentApplicationWorkMode;

            //if (CurrentModeViewModel is IMacrosContent<MacrosCommandNoProtocol> noProtocolInfo)
            if (currentMode == ApplicationWorkMode.NoProtocol)
            {
                var content = new MacrosContent<MacrosCommandNoProtocol>();

                content.MacrosName = MacrosName;
                content.Commands = new List<MacrosCommandNoProtocol>();

                foreach (var command in CommandItems)
                {
                    if (command.ItemData is MacrosCommandNoProtocol itemData)
                    {
                        content.Commands.Add(itemData);
                    }                    
                }

                //var noProtocolMacros = noProtocolInfo.GetContent();

                //noProtocolMacros.Name = CommandName;

                //return noProtocolMacros;

                return content;
            }

            //else if (CurrentModeViewModel is IMacrosContent<MacrosCommandModbus> modbusInfo)
            else if (currentMode == ApplicationWorkMode.ModbusClient)
            {
                var content = new MacrosContent<MacrosCommandModbus>();

                content.MacrosName = MacrosName;
                content.Commands = new List<MacrosCommandModbus>();

                foreach (var command in CommandItems)
                {
                    if (command.ItemData is MacrosCommandModbus itemData)
                    {
                        content.Commands.Add(itemData);
                    }
                }

                //var modbusMacros = modbusInfo.GetContent();

                //modbusMacros.Name = CommandName;

                //return modbusMacros;

                return content;
            }

            else
            {
                throw new NotImplementedException();
            }
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
