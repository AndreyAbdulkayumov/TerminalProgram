using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reactive.Linq;
using TerminalProgramBase.Views;
using TerminalProgramBase.Services;
using ViewModels;
using ViewModels.NoProtocol;
using ViewModels.ModbusClient;
using ViewModels.Macros;
using Services.Interfaces;

namespace TerminalProgramBase;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var serviceCollection = new ServiceCollection()
            // Главное окно
            .AddSingleton<MainWindow_VM>()
            // Компоненты главного окна
            .AddSingleton<NoProtocol_VM>()
            .AddSingleton<NoProtocol_Mode_Normal_VM>()
            .AddSingleton<NoProtocol_Mode_Cycle_VM>()
            .AddSingleton<ModbusClient_VM>()
            .AddSingleton<ModbusClient_Mode_Normal_VM>()
            .AddSingleton<ModbusClient_Mode_Cycle_VM>()
            // Дочерние окна
            .AddTransient<Macros_VM>()
            // MessageBox с разными владельцами
            .AddTransient<IMessageBoxMainWindow, MessageBoxMainWindow>()
            // Вспомогательные сервисы
            .AddSingleton<IUIServices, UIServices>()
            .AddSingleton<IOpenChildWindow, OpenChildWindow>();

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow()
            {
                DataContext = _serviceProvider.GetService<MainWindow_VM>(),
            };

            desktop.MainWindow.Loaded += (object? sender, RoutedEventArgs e) =>
            {
                (desktop.MainWindow.DataContext as MainWindow_VM)?.MainWindowLoadedHandler();
            };

            desktop.MainWindow.Closing += async (object? sender, WindowClosingEventArgs e) =>
            {
                await (desktop.MainWindow.DataContext as MainWindow_VM)?.Command_Closing.Execute();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
