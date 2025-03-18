using ReactiveUI;
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

        public readonly Guid Id;

        private readonly object? _initData;
        private readonly IMessageBox _messageBox;

        public EditCommand_VM(Guid id, EditCommandParameters parameters, IMessageBox messageBox)
        {
            Id = id;

            _initData = parameters.InitData;
            _messageBox = messageBox;

            CommandName = parameters.CommandName;

            CurrentModeViewModel = GetCommandVM();
        }

        public bool IsValidationErrorExist()
        {
            if (CurrentModeViewModel is IMacrosValidation validatedMacros)
            {
                string? message = validatedMacros.GetValidationMessage();

                if (!string.IsNullOrEmpty(message))
                {
                    _messageBox.Show($"Ошибка в команде \"{CommandName}\".\n\n{message}", MessageType.Warning);
                    return true;
                }
            }

            return false;
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
