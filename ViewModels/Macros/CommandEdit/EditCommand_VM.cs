using ReactiveUI;
using System.Reactive;
using MessageBox_Core;
using Core.Models.Settings.FileTypes;
using ViewModels.Macros.DataTypes;
using ViewModels.Macros.CommandEdit.Types;

namespace ViewModels.Macros.CommandEdit
{
    public class EditCommand_VM : ReactiveObject
    {
        private string? _commandName = string.Empty;

        public string? CommandName
        {
            get => _commandName;
            set => this.RaiseAndSetIfChanged(ref _commandName, value);
        }

        private object? _currentModeViewModel;

        public object? CurrentModeViewModel
        {
            get => _currentModeViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentModeViewModel, value);
        }

        public bool Saved { get; private set; } = false;

        public ReactiveCommand<Unit, Unit> Command_SaveMacros { get; }

        private readonly object? _initData;
        private readonly IMessageBox _messageBox;

        public EditCommand_VM(EditCommandParameters parameters, IMessageBox messageBox)
        {
            _initData = parameters.InitData;
            _messageBox = messageBox;

            CommandName = parameters.CommandName;

            Command_SaveMacros = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrWhiteSpace(CommandName))
                {
                    messageBox.Show("Задайте имя команды.", MessageType.Warning);
                    return;
                }

                if (parameters.ExistingCommandNames != null)
                {
                    var macrosNames = parameters.ExistingCommandNames;

                    if (parameters.InitData != null || parameters.CommandName != null)
                    {
                        macrosNames = macrosNames.Where(e => e != parameters.CommandName);
                    }

                    if (macrosNames.Contains(CommandName))
                    {
                        messageBox.Show("Макрос с таким именем уже существует.", MessageType.Warning);
                        return;
                    }
                }

                if (CurrentModeViewModel is IMacrosValidation validatedMacros)
                {
                    string? message = validatedMacros.GetValidationMessage();

                    if (!string.IsNullOrEmpty(message))
                    {
                        messageBox.Show(message, MessageType.Warning);
                        return;
                    }
                }

                Saved = true;
            });
            Command_SaveMacros.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка сохранения макроса.\n\n{error.Message}", MessageType.Error));

            CurrentModeViewModel = GetCommandVM();
        }

        public object GetCommandContent()
        {
            if (CurrentModeViewModel is IMacrosContent<MacrosCommandNoProtocol> noProtocolInfo)
            {
                var noProtocolMacros = noProtocolInfo.GetContent();

                noProtocolMacros.Name = CommandName;

                return noProtocolMacros;
            }

            else if (CurrentModeViewModel is IMacrosContent<MacrosCommandModbus> modbusInfo)
            {
                var modbusMacros = modbusInfo.GetContent();

                modbusMacros.Name = CommandName;

                return modbusMacros;
            }

            else
            {
                throw new NotImplementedException();
            }
        }

        private object GetCommandVM()
        {
            var currentMode = MainWindow_VM.CurrentApplicationWorkMode;

            switch (currentMode)
            {
                case ApplicationWorkMode.NoProtocol:
                    return new NoProtocolCommand_VM(_initData);

                case ApplicationWorkMode.ModbusClient:
                    return new ModbusCommand_VM(_initData, _messageBox);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
