using MessageBox.Core;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.AvaloniaUI.ViewModels;

public enum MessageBoxToolType
{
    Default,
    YesNo
}

public class ButtonContent
{
    public string Content { get; set; }

    public ButtonContent(string Content)
    {
        this.Content = Content;
    }
}

public class MessageBox_VM : ReactiveObject
{
    private string? _title;

    public string? Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string? _content;

    public string? Content
    {
        get => _content;
        set => this.RaiseAndSetIfChanged(ref _content, value);
    }

    private MessageType _type = MessageType.Information;

    public MessageType Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    public ObservableCollection<ButtonContent> Buttons { get; set; } = new ObservableCollection<ButtonContent>();

    public const string Content_OK = "ОК";
    public const string Content_Yes = "Да";
    public const string Content_No = "Нет";

    private bool _errorReportIsVisible;

    public bool ErrorReportIsVisible
    {
        get => _errorReportIsVisible;
        set => this.RaiseAndSetIfChanged(ref _errorReportIsVisible, value);
    }

    public ReactiveCommand<Unit, Unit>? Command_ViewError { get; set; }
    public ReactiveCommand<Unit, Unit>? Command_CopyErrorToClipboard { get; set; }
    public ReactiveCommand<Unit, Unit>? Command_CopyErrorToFile { get; set; }

    private readonly string? _appVersion;

    public MessageBox_VM(Action<string> openErrorReport, Func<string, Task> copyToClipboard, Func<string, Task<string?>> getFolderPath,
        string message, string title, MessageType messageType, MessageBoxToolType toolType, string? appVersion, Exception? error = null)
    {
        Content = message;
        Title = title;

        Type = messageType;

        _appVersion = appVersion;

        if (error != null)
        {
            ErrorReportIsVisible = true;

            DateTime reportDate = DateTime.Now;

            string report = GetFullExceptionInfo(reportDate, error);

            Command_ViewError = ReactiveCommand.Create(() =>
            {
                openErrorReport(report);
            });
            Command_ViewError.ThrownExceptions.Subscribe(error => { });

            Command_CopyErrorToClipboard = ReactiveCommand.CreateFromTask(async () =>
            {
                await copyToClipboard(report);
            });
            Command_CopyErrorToClipboard.ThrownExceptions.Subscribe(error => { });

            Command_CopyErrorToFile = ReactiveCommand.CreateFromTask(async () =>
            {
                string? folderPath = await getFolderPath("Выбор папки для сохранения файла отчета об ошибке.");

                if (folderPath == null)
                {
                    return;
                }

                string fileName = Path.Combine(folderPath, $"Отчет об ошибке {reportDate: yyyyMMdd_HHmmss}.txt");

                File.WriteAllText(fileName, report);
            });
            Command_CopyErrorToFile.ThrownExceptions.Subscribe(error => { });
        }

        switch (toolType)
        {
            case MessageBoxToolType.Default:
                Buttons.Add(new ButtonContent(Content_OK));
                break;

            case MessageBoxToolType.YesNo:
                Buttons.Add(new ButtonContent(Content_Yes));
                Buttons.Add(new ButtonContent(Content_No));
                break;

            default:
                Buttons.Add(new ButtonContent(Content_OK));
                break;
        }
    }

    private string GetFullExceptionInfo(DateTime time, Exception? error, bool isInner = false)
    {
        if (error == null)
            return string.Empty;

        var info = new StringBuilder();

        if (!isInner)
        {
            info.AppendLine($"CoreBus, версия {_appVersion}");
        }

        info.AppendLine("----------------------------------------------------------");

        if (!isInner)
        {
            info.AppendLine("EXCEPTION");
            info.AppendLine($"Time: {time.ToString()}");
        }

        else
        {
            info.AppendLine("INNER EXCEPTION");
        }

        info.AppendLine($"Exception Type: {error.GetType().FullName}\n");
        info.AppendLine($"Message:\n{error.Message}\n");
        info.AppendLine($"Stack Trace:\n{error.StackTrace}");

        if (error.InnerException != null)
        {
            info.AppendLine(GetFullExceptionInfo(time, error.InnerException, true));
        }

        return info.ToString();
    }
}
