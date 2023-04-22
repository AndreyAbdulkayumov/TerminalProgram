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
    public class Http_ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Http_Model Model = new Http_Model();

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


        private readonly ICommand sendRequest;

        public ICommand SendRequest
        {
            get
            {
                return sendRequest;
            }
        }


        public Http_ViewModel()
        {
            sendRequest = new Command(SendRequestAction);
        }

        private async void SendRequestAction(object? _)
        {
            Response = await Model.SendRequest(RequestURI);
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
