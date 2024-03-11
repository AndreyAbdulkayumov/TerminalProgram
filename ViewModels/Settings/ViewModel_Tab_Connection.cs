using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace ViewModels.Settings
{
    public class ViewModel_Tab_Connection : ReactiveObject
    {
        private bool _selected_SerialPort;

        public bool Selected_SerialPort
        {
            get => _selected_SerialPort;
            set => this.RaiseAndSetIfChanged(ref _selected_SerialPort, value);
        }

        private bool _selected_Ethernet;

        public bool Selected_Ethernet
        {
            get => _selected_Ethernet;
            set => this.RaiseAndSetIfChanged(ref _selected_Ethernet, value);
        }

        private object? _currentConnectionViewModel;

        public object? CurrentConnectionViewModel
        {
            get => _currentConnectionViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentConnectionViewModel, value);
        }

        public readonly ViewModel_Tab_Connection_SerialPort Connection_SerialPort_VM;
        public readonly ViewModel_Tab_Connection_Ethernet Connection_Ethernet_VM;


        public ViewModel_Tab_Connection(ViewModel_Settings Main_VM)
        {
            Connection_SerialPort_VM = new ViewModel_Tab_Connection_SerialPort(Main_VM);
            Connection_Ethernet_VM = new ViewModel_Tab_Connection_Ethernet(Main_VM);

            this.WhenAnyValue(x => x.Selected_SerialPort)
                .Where(x => x == true)
                .Subscribe(x =>
                {
                    CurrentConnectionViewModel = Connection_SerialPort_VM;
                });

            this.WhenAnyValue(x => x.Selected_Ethernet)
                .Where(x => x == true)
                .Subscribe(x =>
                {
                    CurrentConnectionViewModel = Connection_Ethernet_VM;
                });
        }
    }
}
