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

namespace View_WPF.ViewModels.MainWindow
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

        public ReactiveCommand<string, string> SendRequest_Command { get; }
        public ReactiveCommand<Unit, Unit> ClearResponse_Command { get; }


        private readonly Model_Http Model = new Model_Http();

        private readonly Action<string, MessageType> Message;

        public ViewModel_Http(Action<string, MessageType> MessageBox)
        {
            Message = MessageBox;

            SendRequest_Command = ReactiveCommand.CreateFromTask<string, string>(Model.SendRequest);

            SendRequest_Command.Subscribe(result => Response = result);

            SendRequest_Command.ThrownExceptions.Subscribe(error => Message?.Invoke(error.Message, MessageType.Error));

            ClearResponse_Command = ReactiveCommand.Create(new Action(() => Response = string.Empty));
        }
    }
}
