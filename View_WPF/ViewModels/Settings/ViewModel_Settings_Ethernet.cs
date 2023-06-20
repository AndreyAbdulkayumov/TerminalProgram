using Core.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace View_WPF.ViewModels.Settings
{
    public class ViewModel_Settings_Ethernet : ReactiveObject
    {
        private string _ip_address = String.Empty;

        public string IP_Address
        {
            get => _ip_address;
            set => this.RaiseAndSetIfChanged(ref _ip_address, value);
        }

        private string _port = String.Empty;

        public string Port
        {
            get => _port;
            set => this.RaiseAndSetIfChanged(ref _port, value);
        }

        private Action<string, MessageType> Message;

        private readonly ConnectedHost Model;

        public ViewModel_Settings_Ethernet(ViewModel_Settings Main_VM, Action<string, MessageType> MessageBox)
        {
            Main_VM.SettingsFileChanged += Main_VM_SettingsFileChanged;

            Message = MessageBox;

            Model = ConnectedHost.Model;

            this.WhenAnyValue(x => x.Port)
                .Where(x => x != null)
                .Where(x => x != string.Empty)                
                .Select(Main_VM.CheckNumber)
                .Subscribe(result => Port = result);
        }

        private void Main_VM_SettingsFileChanged(object? sender, EventArgs e)
        {
            IP_Address = Model.Settings.Connection_IP.IP_Address;
            Port = Model.Settings.Connection_IP.Port;
        }
    }
}
