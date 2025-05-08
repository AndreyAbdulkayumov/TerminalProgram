using ReactiveUI;
using Services.Interfaces;
using System.Collections.ObjectModel;
using System.Reactive;

namespace ViewModels.NoProtocol
{
    public class NoProtocol_Mode_Files_VM : ReactiveObject
    {
        private ObservableCollection<SendFileItem_VM> _files = new ObservableCollection<SendFileItem_VM>();

        public ObservableCollection<SendFileItem_VM> Files
        {
            get => _files;
            set => this.RaiseAndSetIfChanged(ref _files, value);
        }

        public ReactiveCommand<Unit, Unit> Command_AddFile { get; }

        private readonly IMessageBoxMainWindow _messageBox;

        public NoProtocol_Mode_Files_VM(IMessageBoxMainWindow messageBox)
        {
            _messageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));

            Command_AddFile = ReactiveCommand.Create(() =>
            {
                Files.Add(new SendFileItem_VM(Guid.NewGuid(), "filetest.dat",  SendFileHandler, RemoveFileHandler, _messageBox));
            });
        }

        private void SendFileHandler(Guid id)
        {

        }

        private void RemoveFileHandler(Guid id)
        {
            var commandItem = Files.First(e => e.Id == id);

            Files.Remove(commandItem);
        }
    }
}
