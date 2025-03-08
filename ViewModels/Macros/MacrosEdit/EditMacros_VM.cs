using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using DynamicData;
using MessageBox_Core;
using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;
using ViewModels.Macros.DataTypes;
using Services.Interfaces;

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

        private readonly List<string> _allCommandNames = new List<string>();

        private readonly IMessageBox _messageBox;

        public EditMacros_VM(IMessageBoxMacros messageBox)
        {
            _messageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));

            Command_SaveMacros = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrWhiteSpace(MacrosName))
                {
                    _messageBox.Show("Задайте имя макроса.", MessageType.Warning);
                    return;
                }

                Saved = true;

                _messageBox.Show("Настройки макроса сохранены!", MessageType.Information);
            });
            Command_SaveMacros.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка сохранения макроса.\n\n{error.Message}", MessageType.Error));

            Command_RunMacros = ReactiveCommand.Create(() => { });
            Command_RunMacros.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка запуска макроса.\n\n{error.Message}", MessageType.Error));

            Command_AddCommand = ReactiveCommand.Create(() =>
            {
                string defaultName = (CommandItems.Count() + 1).ToString();

                _allCommandNames.Add(defaultName);
                _allCommandParameters.Add(new EditCommandParameters(defaultName, null, _allCommandNames));

                //CommandItems.Add(new MacrosCommandItem_VM(_allCommandParameters.Last(), openEditCommandWindow, RemoveCommand, messageBox));
            });
            Command_AddCommand.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка добавления команды.\n\n{error.Message}", MessageType.Error));

            Command_AddDelay = ReactiveCommand.Create(() => { });
            Command_AddDelay.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка добавления задержки.\n\n{error.Message}", MessageType.Error));
        }

        public void SetParameters(object? macrosParameters)
        {
            IEnumerable<EditCommandParameters>? commands;

            if (macrosParameters is MacrosContent<MacrosCommandNoProtocol> noProtocolContent)
            {
                MacrosName = noProtocolContent.MacrosName;
                commands = noProtocolContent.Commands?.Select(e => new EditCommandParameters(e.Name, e, _allCommandNames));
            }

            else if (macrosParameters is MacrosContent<MacrosCommandModbus> modbusContent)
            {
                MacrosName = modbusContent.MacrosName;
                commands = modbusContent.Commands?.Select(e => new EditCommandParameters(e.Name, e, _allCommandNames));
            }

            else
            {
                throw new NotImplementedException();
            }

            if (commands != null)
            {
                //_allCommandNames = commands.Select(e => e.CommandName).Where(e => e != null).Cast<string>().ToList();
                //_allCommandParameters.AddRange(commands);

                //CommandItems.AddRange(_allCommandParameters.Select(e => new MacrosCommandItem_VM(e, openEditCommandWindow, RemoveCommand, messageBox)));
            }
        }

        public object GetMacrosContent()
        {
            switch (MainWindow_VM.CurrentApplicationWorkMode)
            {
                case ApplicationWorkMode.NoProtocol:
                    return GetNoProtocolMacrosContent();

                case ApplicationWorkMode.ModbusClient:
                    return GetModbusMacrosContent();

                default:
                    throw new NotImplementedException();
            };
        }

        private MacrosContent<MacrosCommandNoProtocol> GetNoProtocolMacrosContent()
        {
            var content = new MacrosContent<MacrosCommandNoProtocol>();

            content.MacrosName = MacrosName;
            content.Commands = new List<MacrosCommandNoProtocol>();

            content.Commands = CommandItems
                .Select(e =>
                {
                    if (e.CommandData is MacrosCommandNoProtocol data && data.Content != null)
                    {
                        return new MacrosCommandNoProtocol()
                        {
                            Name = e.CommandName,
                            Content = data.Content,                            
                        };
                    }

                    return new MacrosCommandNoProtocol()
                    {
                        Name = e.CommandName,
                        Content = null,
                    };
                })
                .ToList();

            return content;
        }

        private MacrosContent<MacrosCommandModbus> GetModbusMacrosContent()
        {
            var content = new MacrosContent<MacrosCommandModbus>();

            content.MacrosName = MacrosName;
            content.Commands = new List<MacrosCommandModbus>();

            content.Commands = CommandItems
                .Select(e =>
                {
                    if (e.CommandData is MacrosCommandModbus data && data.Content != null)
                    {
                        return new MacrosCommandModbus()
                        {
                            Name = e.CommandName,
                            Content = data.Content,
                        };
                    }

                    return new MacrosCommandModbus()
                    {
                        Name = e.CommandName,
                        Content = null,
                    };
                })
                .ToList();

            return content;
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
