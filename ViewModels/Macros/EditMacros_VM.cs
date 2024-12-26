using Core.Models.Settings.FileTypes;
using MessageBox_Core;
using ReactiveUI;
using System.Reactive;
using ViewModels.Macros.DataTypes;
using ViewModels.Macros.MacrosEdit;

namespace ViewModels.Macros
{
    public class EditMacros_VM : ReactiveObject
    {
        private string? _macrosName = string.Empty;

        public string? MacrosName
        {
            get => _macrosName;
            set => this.RaiseAndSetIfChanged(ref _macrosName, value);
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

        public EditMacros_VM(EditMacrosParameters parameters, Action closeWindowAction, IMessageBox messageBox)
        {
            _initData = parameters.InitData;

            MacrosName = parameters.MacrosName;

            Command_SaveMacros = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrWhiteSpace(MacrosName))
                {
                    messageBox.Show("Задайте имя макроса.", MessageType.Warning);
                    return;
                }

                if (parameters.InitData == null && parameters.ExistingMacrosNames != null && parameters.ExistingMacrosNames.Contains(MacrosName))
                {
                    messageBox.Show("Макрос с таким именем уже существует.", MessageType.Warning);
                    return;
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
                closeWindowAction();
            });

            CurrentModeViewModel = GetMacrosVM();
        }

        public object GetMacrosContent()
        {
            if (CurrentModeViewModel is IMacrosContent<MacrosNoProtocolItem> noProtocolInfo)
            {
                var noProtocolMacros = noProtocolInfo.GetContent();

                noProtocolMacros.Name = MacrosName;

                return noProtocolMacros;
            }

            else if (CurrentModeViewModel is IMacrosContent<MacrosModbusItem> modbusInfo)
            {
                var modbusMacros = modbusInfo.GetContent();

                modbusMacros.Name = MacrosName;

                return modbusMacros;
            }

            else
            {
                throw new NotImplementedException();
            }
        }

        private object GetMacrosVM()
        {
            var currentMode = CommonUI_VM.CurrentApplicationWorkMode;

            switch (currentMode)
            {
                case ApplicationWorkMode.NoProtocol:
                    return new NoProtocolMacros_VM(_initData);

                case ApplicationWorkMode.ModbusClient:
                    return new ModbusMacros_VM(_initData);

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
