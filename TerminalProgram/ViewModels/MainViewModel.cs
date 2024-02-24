using ReactiveUI;
using System.Reactive;
using TerminalProgram.Views.Protocols;

namespace TerminalProgram.ViewModels;

public class MainViewModel : ViewModelBase
{
    private object? _currentView;

    public object? CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    private readonly NoProtocol NoProtocol_View = new NoProtocol();
    private readonly Modbus_Client Modbus_Client_View = new Modbus_Client();
    private readonly Modbus_Server Modbus_Server_View = new Modbus_Server();


    private ReactiveCommand<Unit, Unit> Select_NoProtocol_View { get; }
    private ReactiveCommand<Unit, Unit> Select_Modbus_Client_View { get; }
    private ReactiveCommand<Unit, Unit> Select_Modbus_Server_View { get; }

    public MainViewModel()
    {
        CurrentView = NoProtocol_View;

        Select_NoProtocol_View = ReactiveCommand.Create(() => { CurrentView = NoProtocol_View; });
        Select_Modbus_Client_View = ReactiveCommand.Create(() => { CurrentView = Modbus_Client_View; });
        Select_Modbus_Server_View = ReactiveCommand.Create(() => { CurrentView = Modbus_Server_View; });
    }
}
