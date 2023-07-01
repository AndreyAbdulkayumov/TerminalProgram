using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Models.Http;
using ReactiveUI;
using TerminalProgram.Views;

namespace TerminalProgram.ViewModels.MainWindow
{
    public class ViewModel_Http : ReactiveObject
    {
        private string requestURI = string.Empty;

        public string RequestURI
        {
            get { return requestURI; }
            set { this.RaiseAndSetIfChanged(ref requestURI, value); }
        }

        private string response = string.Empty;

        public string Response
        {
            get { return response; }
            set { this.RaiseAndSetIfChanged(ref response, value); }
        }

        public ReactiveCommand<string, string> Command_SendRequest { get; }
        public ReactiveCommand<Unit, Unit> Command_ClearResponse { get; }


        private readonly Model_Http Model = new Model_Http();

        private readonly Action<string, MessageType> Message;

        public ViewModel_Http(Action<string, MessageType> MessageBox)
        {
            Message = MessageBox;

            Command_SendRequest = ReactiveCommand.CreateFromTask<string, string>(Model.SendRequest);
            Command_SendRequest.Subscribe(result => Response = result);
            Command_SendRequest.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));

            Command_ClearResponse = ReactiveCommand.Create(new Action(() => Response = string.Empty));
            Command_ClearResponse.ThrownExceptions.Subscribe(error => Message.Invoke(error.Message, MessageType.Error));
        }
    }
}
