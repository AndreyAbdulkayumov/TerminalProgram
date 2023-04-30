using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Core.Models.Http;

namespace Core.ViewModels
{
    public class ViewModel_Http : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        #region Properties

        private string requestURI = String.Empty;

        public string RequestURI
        {
            get
            {
                return requestURI;
            }

            set
            {
                requestURI = value;
                OnPropertyChanged(nameof(RequestURI));
            }
        }

        private string response = String.Empty;

        public string Response
        {
            get
            {
                return response;
            }

            set
            {
                response = value; 
                OnPropertyChanged(nameof(Response));
            }
        }

        #endregion

        #region Commands

        public ICommand SendRequest_Command { get; }
        public ICommand ClearResponse_Command { get; }

        #endregion

        private Model_Http Model = new Model_Http();

        private ViewMessage Message;

        public ViewModel_Http(ViewMessage MessageBox)
        {
            Message = MessageBox;

            SendRequest_Command = new ButtonCommand(
                ExecuteAction: SendRequest_Action,
                MessageBox: Message
                );

            ClearResponse_Command = new ButtonCommand(
                ExecuteAction: ClearResponse_Action,
                MessageBox: Message
                );
        }

        private async void SendRequest_Action(object? _)
        {
            try
            {
                Response = await Model.SendRequest(RequestURI);
            }
            
            catch (Exception error)
            {
                Message?.Invoke(error.Message, MessageType.Error);
            }
        }

        private void ClearResponse_Action(object? _)
        {
            Response = String.Empty;
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
