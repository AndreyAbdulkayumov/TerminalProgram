using Core.Models;
using Core.Models.Http;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace View_WPF.ViewModels
{
    public class ViewModel_CommonUI : ReactiveObject
    {
        public bool IsConnect
        {
            get { return Model.HostIsConnect; }
        }

        public string SettingsDocument
        {
            get
            {
                Properties.Settings.Default.Reload();
                return Properties.Settings.Default.SettingsDocument;
            }

            set
            {
                Properties.Settings.Default.SettingsDocument = value;
                Properties.Settings.Default.Save();
            }
        }

        public ReactiveCommand<Unit, Unit> Command_ProtocolMode_NoProtocol { get; }
        public ReactiveCommand<Unit, Unit> Command_ProtocolMode_Modbus { get; }

        public ReactiveCommand<string, Unit> Command_Connect { get; }
        public ReactiveCommand<Unit, Unit> Command_Disconnect { get; }


        private readonly ConnectedHost Model;

        private readonly ViewMessage Message;

        public ViewModel_CommonUI(ViewMessage MessageBox)
        {
            Model = ConnectedHost.Model;

            /**********************************/
            //
            // Debug
            //
            SettingsDocument = "Unknown";

            //
            /**********************************/

            Message = MessageBox;

            Command_ProtocolMode_NoProtocol = ReactiveCommand.Create(Model.SetProtocol_NoProtocol);
            Command_ProtocolMode_Modbus = ReactiveCommand.Create(Model.SetProtocol_Modbus);

            Command_Connect = ReactiveCommand.CreateFromTask(new Func<string, Task>(Model.Connect));

            Command_Connect.ThrownExceptions.Subscribe(error => Message?.Invoke(error.Message, MessageType.Error));

            Command_Disconnect = ReactiveCommand.CreateFromTask(Model.Disconnect);

            Command_Disconnect.ThrownExceptions.Subscribe(error => Message?.Invoke(error.Message, MessageType.Error));
        }
    }
}
