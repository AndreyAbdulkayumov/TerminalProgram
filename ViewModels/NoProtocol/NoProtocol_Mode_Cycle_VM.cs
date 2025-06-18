using ReactiveUI;
using System.Reactive;
using MessageBox.Core;
using Core.Models;
using Core.Models.NoProtocol.DataTypes;
using Core.Clients.DataTypes;
using ViewModels.Helpers;
using Services.Interfaces;
using Core.Models.NoProtocol;

namespace ViewModels.NoProtocol;

public class NoProtocol_Mode_Cycle_VM : ReactiveObject
{
    private bool ui_IsEnable = false;

    public bool UI_IsEnable
    {
        get => ui_IsEnable;
        set => this.RaiseAndSetIfChanged(ref ui_IsEnable, value);
    }

    private bool _isStart = false;

    public bool IsStart
    {
        get => _isStart;
        set => this.RaiseAndSetIfChanged(ref _isStart, value);
    }

    #region Message

    private bool _isBytesSend;

    public bool IsBytesSend
    {
        get => _isBytesSend;
        set => this.RaiseAndSetIfChanged(ref _isBytesSend, value);
    }

    private string _message_Content = string.Empty;

    public string Message_Content
    {
        get => _message_Content;
        set => this.RaiseAndSetIfChanged(ref _message_Content, value);
    }

    private bool _message_CR = false;

    public bool Message_CR
    {
        get => _message_CR;
        set => this.RaiseAndSetIfChanged(ref _message_CR, value);
    }

    private bool _message_LF = false;

    public bool Message_LF
    {
        get => _message_LF;
        set => this.RaiseAndSetIfChanged(ref _message_LF, value);
    }

    private int _message_Period_ms = 100;

    public int Message_Period_ms
    {
        get => _message_Period_ms;
        set => this.RaiseAndSetIfChanged(ref _message_Period_ms, value);
    }


    #endregion

    #region Response

    private bool _response_Date = false;

    public bool Response_Date
    {
        get => _response_Date;
        set => this.RaiseAndSetIfChanged(ref _response_Date, value);
    }

    private bool _response_Time = false;

    public bool Response_Time
    {
        get => _response_Time;
        set => this.RaiseAndSetIfChanged(ref _response_Time, value);
    }

    private bool _response_String_Start_Enable = false;

    public bool Response_String_Start_Enable
    {
        get => _response_String_Start_Enable;
        set => this.RaiseAndSetIfChanged(ref _response_String_Start_Enable, value);
    }

    private string _response_String_Start = string.Empty;

    public string Response_String_Start
    {
        get => _response_String_Start;
        set => this.RaiseAndSetIfChanged(ref _response_String_Start, value);
    }

    private bool _response_String_End_Enable = false;

    public bool Response_String_End_Enable
    {
        get => _response_String_End_Enable;
        set => this.RaiseAndSetIfChanged(ref _response_String_End_Enable, value);
    }

    private string _response_String_End = string.Empty;

    public string Response_String_End
    {
        get => _response_String_End;
        set => this.RaiseAndSetIfChanged(ref _response_String_End, value);
    }

    private bool _response_NextLine = false;

    public bool Response_NextLine
    {
        get => _response_NextLine;
        set => this.RaiseAndSetIfChanged(ref _response_NextLine, value);
    }

    #endregion

    #region Button

    private const string Button_Content_Start = "Начать опрос";
    private const string Button_Content_Stop = "Остановить опрос";

    private string _button_Content = Button_Content_Start;

    public string Button_Content
    {
        get => _button_Content;
        set => this.RaiseAndSetIfChanged(ref _button_Content, value);
    }

    public ReactiveCommand<Unit, Unit> Command_Start_Stop_Polling { get; }

    #endregion

    private readonly IMessageBoxMainWindow _messageBox;
    private readonly ConnectedHost _connectedHostModel;
    private readonly Model_NoProtocol _noProtocolModel;

    public NoProtocol_Mode_Cycle_VM(IMessageBoxMainWindow messageBox, ConnectedHost connectedHostModel, Model_NoProtocol noProtocolModel)
    {
        _messageBox = messageBox ?? throw new ArgumentNullException(nameof(messageBox));
        _connectedHostModel = connectedHostModel ?? throw new ArgumentNullException(nameof(connectedHostModel));
        _noProtocolModel = noProtocolModel ?? throw new ArgumentNullException(nameof(noProtocolModel));

        _connectedHostModel.DeviceIsConnect += Model_DeviceIsConnect;
        _connectedHostModel.DeviceIsDisconnected += Model_DeviceIsDisconnected;

        _noProtocolModel.Model_ErrorInCycleMode += NoProtocol_Model_ErrorInCycleMode;

        Command_Start_Stop_Polling = ReactiveCommand.CreateFromTask(async () =>
        {
            if (IsStart)
            {
                StopPolling();
                return;
            }

            await StartPolling();
        });
        Command_Start_Stop_Polling.ThrownExceptions.Subscribe(error => _messageBox.Show(error.Message, MessageType.Error, error));

        this.WhenAnyValue(x => x.IsBytesSend)
            .Subscribe(IsBytes =>
            {
                Message_Content = StringByteConverter.GetMessageString(Message_Content, IsBytes, _noProtocolModel.HostEncoding);
            });
    }

    public string GetValidatedString()
    {
        if (IsBytesSend)
        {
            return StringByteConverter.GetValidatedByteString(Message_Content);
        }

        return Message_Content;
    }

    private void Model_DeviceIsConnect(object? sender, IConnection? e)
    {
        UI_IsEnable = true;
    }

    private void Model_DeviceIsDisconnected(object? sender, IConnection? e)
    {
        UI_IsEnable = false;

        StopPolling();
    }

    private void NoProtocol_Model_ErrorInCycleMode(object? sender, Exception e)
    {
        _messageBox.Show($"Ошибка отправки команды в цикличном опросе.\n\n{e.Message}\n\nОпрос остановлен.", MessageType.Error, e);
    }

    public void SourceWindowClosingAction()
    {
        _noProtocolModel.CycleMode_Stop();
        _noProtocolModel.Model_ErrorInCycleMode -= NoProtocol_Model_ErrorInCycleMode;
    }

    public void StopPolling()
    {
        _noProtocolModel.CycleMode_Stop();

        Button_Content = Button_Content_Start;
        IsStart = false;
    }

    private async Task StartPolling()
    {
        byte[] buffer = NoProtocol_VM.CreateSendBuffer(_isBytesSend, Message_Content, Message_CR, Message_LF, _noProtocolModel.HostEncoding);

        _noProtocolModel.CycleMode_Period = Message_Period_ms;

        var info = new CycleModeParameters(
            messageBytes: buffer,
            response_Date_Enable: Response_Date,
            response_Time_Enable: Response_Time,
            response_String_Start_Enable: Response_String_Start_Enable,
            response_String_Start: Response_String_Start,
            response_String_End_Enable: Response_String_End_Enable,
            response_String_End: Response_String_End,
            response_NextLine_Enable: Response_NextLine
            );

        await _noProtocolModel.CycleMode_Start(info);

        Button_Content = Button_Content_Stop;
        IsStart = true;
    }
}
