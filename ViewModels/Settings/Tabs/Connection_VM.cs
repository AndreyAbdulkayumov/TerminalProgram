using MessageBox_Core;
using ReactiveUI;
using System.Reactive.Linq;

namespace ViewModels.Settings.Tabs
{
    public class Connection_VM : ReactiveObject
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

        public readonly Connection_SerialPort_VM Connection_SerialPort_VM;
        public readonly Connection_Ethernet_VM Connection_Ethernet_VM;


        public Connection_VM(Settings_VM main_VM, IMessageBox messageBox)
        {
            Connection_SerialPort_VM = new Connection_SerialPort_VM(main_VM, messageBox);
            Connection_Ethernet_VM = new Connection_Ethernet_VM(main_VM, messageBox);

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
