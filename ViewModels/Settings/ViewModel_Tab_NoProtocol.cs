using ReactiveUI;
using System.Collections.ObjectModel;

namespace ViewModels.Settings
{
    public class ViewModel_Tab_NoProtocol : ReactiveObject
    {
        private readonly ObservableCollection<string> _typeOfEncoding = new ObservableCollection<string>()
        {
            "ASCII", "UTF-8", "UTF-32", "Unicode"
        };

        public ObservableCollection<string> TypeOfEncoding
        {
            get => _typeOfEncoding;
        }

        private string _selectedEncoding = string.Empty;

        public string SelectedEncoding
        {
            get => _selectedEncoding;
            set => this.RaiseAndSetIfChanged(ref _selectedEncoding, value);
        }


        public ViewModel_Tab_NoProtocol()
        {

        }
    }
}
