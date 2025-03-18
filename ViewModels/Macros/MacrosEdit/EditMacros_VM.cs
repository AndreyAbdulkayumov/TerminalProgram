using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using MessageBox_Core;
using Core.Models.Settings.DataTypes;
using Core.Models.Settings.FileTypes;
using ViewModels.Macros.DataTypes;
using Services.Interfaces;
using ViewModels.Macros.CommandEdit;
using Core.Models.Modbus.DataTypes;
using ViewModels.Helpers;
using ViewModels.ModbusClient.MessageBusTypes;
using ViewModels.NoProtocol.DataTypes;

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

        private bool _isEdit;

        public bool IsEdit
        {
            get => _isEdit;
            set => this.RaiseAndSetIfChanged(ref _isEdit, value);
        }

        private object? _editCommandViewModel;

        public object? EditCommandViewModel
        {
            get => _editCommandViewModel;
            set => this.RaiseAndSetIfChanged(ref _editCommandViewModel, value);
        }

        public string EmptyCommandMessage => "Выберите команду для редактирования";

        public ReactiveCommand<Unit, Unit> Command_SaveMacros { get; }
        public ReactiveCommand<Unit, Unit> Command_RunMacros { get; }
        public ReactiveCommand<Unit, Unit> Command_AddCommand { get; }
        public ReactiveCommand<Unit, Unit> Command_AddDelay { get; }

        public bool Saved { get; private set; } = false;

        
        private readonly IMessageBox _messageBox;

        private List<EditCommand_VM> _allEditCommandVM = new List<EditCommand_VM>();       


        public EditMacros_VM(IMessageBoxEditMacros messageBox)
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

            Command_RunMacros = ReactiveCommand.Create(RunMacros);
            Command_RunMacros.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка запуска макроса.\n\n{error.Message}", MessageType.Error));

            Command_AddCommand = ReactiveCommand.Create(() =>
            {
                string defaultName = (CommandItems.Count() + 1).ToString();

                var commandParameters = new EditCommandParameters(defaultName, null);

                var itemGuid = Guid.NewGuid();

                CommandItems.Add(new MacrosCommandItem_VM(itemGuid, commandParameters, RunCommand, EditCommand, RemoveCommand, _messageBox));
                _allEditCommandVM.Add(new EditCommand_VM(itemGuid, commandParameters, _messageBox));
            });
            Command_AddCommand.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка добавления команды.\n\n{error.Message}", MessageType.Error));

            Command_AddDelay = ReactiveCommand.Create(() => { });
            Command_AddDelay.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка добавления задержки.\n\n{error.Message}", MessageType.Error));

            this.WhenAnyValue(x => x.EditCommandViewModel)
                .Subscribe(x =>
                {
                    IsEdit = x != null ? true : false;
                });
        }

        public void SetParameters(object? macrosParameters)
        {
            IEnumerable<EditCommandParameters>? commands;

            if (macrosParameters is MacrosContent<MacrosCommandNoProtocol> noProtocolContent)
            {
                MacrosName = noProtocolContent.MacrosName;
                commands = noProtocolContent.Commands?.Select(e => new EditCommandParameters(e.Name, e));
            }

            else if (macrosParameters is MacrosContent<MacrosCommandModbus> modbusContent)
            {
                MacrosName = modbusContent.MacrosName;
                commands = modbusContent.Commands?.Select(e => new EditCommandParameters(e.Name, e));
            }

            else
            {
                throw new NotImplementedException();
            }

            if (commands != null)
            {
                foreach (var commandParameters in commands)
                {
                    var itemGuid = Guid.NewGuid();

                    CommandItems.Add(new MacrosCommandItem_VM(itemGuid, commandParameters, RunCommand, EditCommand, RemoveCommand, _messageBox));
                    _allEditCommandVM.Add(new EditCommand_VM(itemGuid, commandParameters, _messageBox));
                }
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
        
        private void RunMacros()
        {
            bool validationErrorIsExist = false;

            foreach(var command in _allEditCommandVM)
            {
                if (command.IsValidationErrorExist())
                {
                    validationErrorIsExist = true;
                    break;
                }
            }

            if (validationErrorIsExist)
            {
                return;
            }

            switch (MainWindow_VM.CurrentApplicationWorkMode)
            {
                case ApplicationWorkMode.NoProtocol:
                    var noProtocolContent = GetNoProtocolMacrosContent();
                    MessageBus.Current.SendMessage(noProtocolContent.Commands);
                    break;

                case ApplicationWorkMode.ModbusClient:
                    var modbusContent = GetModbusMacrosContent();
                    MessageBus.Current.SendMessage(modbusContent.Commands);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private MacrosContent<MacrosCommandNoProtocol> GetNoProtocolMacrosContent()
        {
            var content = new MacrosContent<MacrosCommandNoProtocol>();

            content.MacrosName = MacrosName;
            content.Commands = new List<MacrosCommandNoProtocol>();

            content.Commands = _allEditCommandVM
                .Select(e =>
                {
                    object content = e.GetCommandContent();

                    if (content is MacrosCommandNoProtocol data && data.Content != null)
                    {
                        return data;
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

            content.Commands = _allEditCommandVM
                .Select(e =>
                {
                    object content = e.GetCommandContent();

                    if (content is MacrosCommandModbus data && data.Content != null)
                    {
                        return data;
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

        private void EditCommand(Guid selectedId)
        {
            var commandItem = CommandItems.First(e => e.Id == selectedId);
            var commandVM = _allEditCommandVM.First(e => e.Id == selectedId);

            // Перед каждой сменой VM нужно обнулять ссылку, иначе будет некорректное поведение RadioButton,
            // а также, возможно, и других элементов с типом привязки TwoWay.
            EditCommandViewModel = null;

            if (commandItem.IsEdit)
            {
                commandItem.IsEdit = false;
                return;
            }

            foreach (var item in CommandItems)
            {
                item.IsEdit = false;
            }
            
            commandItem.IsEdit = true;
            EditCommandViewModel = commandVM;
        }

        private void RemoveCommand(Guid selectedId)
        {
            var commandItem = CommandItems.First(e => e.Id == selectedId);
            var commandVM = _allEditCommandVM.First(e => e.Id == selectedId);

            if (commandItem.IsEdit)
            {
                EditCommandViewModel = null;
            }

            CommandItems.Remove(commandItem);
            _allEditCommandVM.Remove(commandVM);
        }

        private void RunCommand(Guid selectedId)
        {
            var commandVM = _allEditCommandVM.First(e => e.Id == selectedId);

            if (commandVM.IsValidationErrorExist())
            {
                return;
            }

            object content = commandVM.GetCommandContent();

            if (content is MacrosCommandNoProtocol noProtocolData && noProtocolData.Content != null)
            {
                RunNoProtocolCommand(noProtocolData.Content);
            }

            else if (content is MacrosCommandModbus modbusData && modbusData.Content != null)
            {
                RunModbusCommand(modbusData.Content);
            }
        }

        private void RunNoProtocolCommand(NoProtocolCommandInfo content)
        {
            MessageBus.Current.SendMessage(
                new NoProtocolSendMessage(content.IsByteString, content.Message, content.EnableCR, content.EnableLF, AppEncoding.GetEncoding(content.MacrosEncoding))
                );
        }

        private void RunModbusCommand(ModbusCommandInfo content)
        {
            var selectedFunction = Function.AllFunctions.First(func => func.Number == content.FunctionNumber);

            if (selectedFunction is ModbusReadFunction readFunction)
            {
                MessageBus.Current.SendMessage(
                    new ModbusReadMessage(content.SlaveID, content.Address, readFunction, content.NumberOfReadRegisters, content.CheckSum_IsEnable)
                    );

                return;
            }

            else if (selectedFunction is ModbusWriteFunction writeFunction)
            {
                MessageBus.Current.SendMessage(
                    new ModbusWriteMessage(content.SlaveID, content.Address, writeFunction, content.WriteInfo?.WriteBuffer, content.NumberOfReadRegisters, content.CheckSum_IsEnable)
                    );

                return;
            }

            throw new Exception($"Задан неизвестный тип функции Modbus.\nКод: {content.FunctionNumber}");
        }
    }
}
