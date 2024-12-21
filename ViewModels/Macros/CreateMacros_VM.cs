using ReactiveUI;
using ViewModels.Macros.MacrosEdit;

namespace ViewModels.Macros
{
    public class CreateMacros_VM : ReactiveObject
    {
        private object? _currentModeViewModel;

        public object? CurrentModeViewModel
        {
            get => _currentModeViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentModeViewModel, value);
        }

        public CreateMacros_VM()
        {
            CurrentModeViewModel = GetMacrosVM();
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
