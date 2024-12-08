using ReactiveUI;
using ViewModels.Macros.MacrosCreate;

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

        private readonly NoProtocolMacros_VM _noProtocolMacros_VM;
        private readonly ModbusMacros_VM _modbusMacros_VM;

        public CreateMacros_VM()
        {
            _noProtocolMacros_VM = new NoProtocolMacros_VM();
            _modbusMacros_VM = new ModbusMacros_VM();

            CurrentModeViewModel = _noProtocolMacros_VM;
        }
    }
}
