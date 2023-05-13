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

        public ReactiveCommand<string, Unit> Command_Connect { get; }
        public ReactiveCommand<Unit, Unit> Command_Disconnect { get; }


        private readonly ConnectedHost Model = new ConnectedHost();

        private readonly ViewMessage Message;

        public ViewModel_CommonUI(ViewMessage MessageBox)
        {
            Message = MessageBox;

            Command_Connect = ReactiveCommand.Create(new Action<string>(Model.Connect));

            Command_Connect.ThrownExceptions.Subscribe(error => Message?.Invoke(error.Message, MessageType.Error));

            Command_Disconnect = ReactiveCommand.CreateFromTask(Model.Disconnect);

            Command_Disconnect.ThrownExceptions.Subscribe(error => Message?.Invoke(error.Message, MessageType.Error));
        }
    }
}
