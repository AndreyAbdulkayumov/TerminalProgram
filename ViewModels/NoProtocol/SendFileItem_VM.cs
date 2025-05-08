using MessageBox_Core;
using ReactiveUI;
using System.Reactive;

namespace ViewModels.NoProtocol
{
    public class SendFileItem_VM : ReactiveObject
    {
        private string? _fileName;

        private string? FileName
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        private string? _fileExtension;

        private string? FileExtension
        {
            get => _fileExtension;
            set => this.RaiseAndSetIfChanged(ref _fileExtension, value);
        }

        public ReactiveCommand<Unit, Unit> Command_SendFile { get; }
        public ReactiveCommand<Unit, Unit> Command_RemoveFile { get; }

        public readonly Guid Id;

        public SendFileItem_VM(Guid id, string filePath, Action<Guid> sendFileHandler, Action<Guid> removeFileHandler, IMessageBox messageBox)
        {
            Id = id;

            FileName = Path.GetFileNameWithoutExtension(filePath);
            FileExtension = Path.GetExtension(filePath);

            Command_SendFile = ReactiveCommand.Create(() => sendFileHandler(id));
            Command_SendFile.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка отправки файла \"{FileName}\".\n\n{error.Message}", MessageType.Error, error));

            Command_RemoveFile = ReactiveCommand.CreateFromTask(async () =>
            {
                if (await messageBox.ShowYesNoDialog($"Вы действительно хотите удалить файл \"{FileName}\"?", MessageType.Warning) == MessageBoxResult.Yes)
                {
                    removeFileHandler(id);
                }
            });
            Command_RemoveFile.ThrownExceptions.Subscribe(error => messageBox.Show($"Ошибка удаления файла \"{FileName}\".\n\n{error.Message}", MessageType.Error, error));
        }
    }
}
