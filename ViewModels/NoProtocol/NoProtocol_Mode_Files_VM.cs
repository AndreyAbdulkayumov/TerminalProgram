using Core.Models.Settings;
using MessageBox_Core;
using ReactiveUI;
using Services.Interfaces;
using System.Collections.ObjectModel;
using System.Reactive;

namespace ViewModels.NoProtocol;

public class NoProtocol_Mode_Files_VM : ReactiveObject
{
    private ObservableCollection<SendFileItem_VM> _files = new ObservableCollection<SendFileItem_VM>();

    public ObservableCollection<SendFileItem_VM> Files
    {
        get => _files;
        set => this.RaiseAndSetIfChanged(ref _files, value);
    }

    public ReactiveCommand<Unit, Unit> Command_AddFile { get; }
    public ReactiveCommand<Unit, Unit> Command_RemoveAllFiles { get; }

    private readonly IFileSystemService _fileSystemService;
    private readonly IMessageBoxMainWindow _messageBox;
    private readonly Model_Settings _settingsModel;

    public NoProtocol_Mode_Files_VM(IFileSystemService fileSystemService, IMessageBoxMainWindow messageBox,
        Model_Settings settingsModel)
    {
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _messageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));
        _settingsModel = settingsModel ?? throw new ArgumentNullException(nameof(settingsModel));

        Command_AddFile = ReactiveCommand.CreateFromTask(AddFileHandler);
        Command_AddFile.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка добавления файла.\n\n{error.Message}", MessageType.Error, error));

        Command_RemoveAllFiles = ReactiveCommand.CreateFromTask(RemoveAllFiles);
        Command_RemoveAllFiles.ThrownExceptions.Subscribe(error => _messageBox.Show($"Ошибка удаления всех файлов.\n\n{error.Message}", MessageType.Error, error));

        UpdateFileList();
    }

    private async Task AddFileHandler()
    {
        string? filePath = await _fileSystemService.GetFilePath($"Выбор файла для отправки.", "Файл отправки", null);

        if (string.IsNullOrEmpty(filePath))
            return;

        string targetFilePath = _settingsModel.CopySendFileInWorkDirectory(filePath);

        Files.Add(new SendFileItem_VM(Guid.NewGuid(), targetFilePath, SendFileHandler, RemoveFileHandler, _messageBox));
    }

    private async Task RemoveAllFiles()
    {
        if (Files.Count() == 0)
            return;

        if (await _messageBox.ShowYesNoDialog("Вы действительно хотите удалить все файлы?", MessageType.Warning) == MessageBoxResult.Yes)
        {
            foreach (var file in Files)
            {
                _settingsModel.DeleteFile(file.FilePath);
            }

            Files.Clear();
        }
    }

    private void SendFileHandler(Guid id)
    {

    }

    private void RemoveFileHandler(Guid id)
    {
        var commandItem = Files.First(e => e.Id == id);

        _settingsModel.DeleteFile(commandItem.FilePath);

        Files.Remove(commandItem);
    }

    private void UpdateFileList()
    {
        var files = _settingsModel.GetAllSendFilesNames();

        foreach (var filePath in files)
        {
            Files.Add(new SendFileItem_VM(Guid.NewGuid(), filePath, SendFileHandler, RemoveFileHandler, _messageBox));
        }
    }
}
