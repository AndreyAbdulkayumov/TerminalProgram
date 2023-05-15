using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using ReactiveUI;

namespace View_WPF.ViewModels
{
    public class ViewModel_NoProtocol : ReactiveObject
    {
        private bool _cr_enable;

        public bool CR_Enable
        {
            get => _cr_enable;
            set => this.RaiseAndSetIfChanged(ref _cr_enable, value);
        }

        private bool _lf_enable;

        public bool LF_Enable
        {
            get => _lf_enable;
            set => this.RaiseAndSetIfChanged(ref _lf_enable, value);
        }

        private bool _messageIsChar;

        public bool MessageIsChar
        {
            get => _messageIsChar;
            set => this.RaiseAndSetIfChanged(ref _messageIsChar, value);
        }

        private bool _messageIsString;

        public bool MessageIsString
        {
            get => _messageIsString;
            set => this.RaiseAndSetIfChanged(ref _messageIsString, value);
        }

        public ReactiveCommand<string, Unit> Command_Send { get; }

        private readonly ConnectedHost Model;

        private readonly ViewMessage Message;


        public ViewModel_NoProtocol(ViewMessage MessageBox)
        {
            Message = MessageBox;

            Model = ConnectedHost.Model;

            Command_Send = ReactiveCommand.Create<string>(SendMessage);

            Command_Send.ThrownExceptions.Subscribe(error => Message?.Invoke(error.Message, MessageType.Error));
        }


        public void SendMessage(string Message)
        {
            Model.NoProtocol.Send(Message, CR_Enable, LF_Enable);

            //if (MessageIsChar == true && MessageIsString == false)
            //{
            //    Model.NoProtocol.Send(Message.Last().ToString(), CR_Enable, LF_Enable);
            //}

            //else if (MessageIsChar == false && MessageIsString == true)
            //{
            //    Model.NoProtocol.Send(Message, CR_Enable, LF_Enable);
            //}
        }
    }
}
