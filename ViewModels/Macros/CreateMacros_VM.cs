using Core.Models.Settings.FileTypes;
using ReactiveUI;
using System.Reactive;
using ViewModels.Macros.MacrosEdit;

namespace ViewModels.Macros
{
    public class CreateMacros_VM : ReactiveObject
    {
        private string _macrosName = string.Empty;

        public string MacrosName
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

        public CreateMacros_VM(Action closeWindowAction)
        {
            Command_SaveMacros = ReactiveCommand.Create(() =>
            {
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
                    return new NoProtocolMacros_VM();

                case ApplicationWorkMode.ModbusClient:
                    return new ModbusMacros_VM();

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
