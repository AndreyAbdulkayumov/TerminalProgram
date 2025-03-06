using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Services.Interfaces;
using System;
using System.Threading.Tasks;
using TerminalProgramBase.Views;
using TerminalProgramBase.Views.Settings;
using ViewModels.Settings;

namespace TerminalProgramBase.Services
{
    public class OpenChildWindowService : IOpenChildWindowService
    {
        private Settings_VM? _settingsVM;

        private const double WorkspaceOpacity_OpenChildWindow = 0.15;

        private readonly IServiceProvider _serviceProvider;

        public OpenChildWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Settings()
        {
            _settingsVM = _serviceProvider.GetService<Settings_VM>();

            if (_settingsVM == null)
            {
                return;
            }

            var window = new SettingsWindow()
            {
                DataContext = _settingsVM
            };

            window.Loaded += SettingsWindow_Loaded;
            window.KeyDown += SettingsWindow_KeyDown;

            await OpenWindowWithDimmer(window, MainWindow.Instance, MainWindow.Workspace);

            window.Loaded -= SettingsWindow_Loaded;
            window.KeyDown -= SettingsWindow_KeyDown;

            _settingsVM.WindowClosed();
        }

        private void SettingsWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            _settingsVM?.WindowLoaded();
        }

        private void SettingsWindow_KeyDown(object? sender, KeyEventArgs e)
        {
            var settingsWindow = sender as SettingsWindow;

            if (settingsWindow == null)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Enter:
                    _settingsVM?.Enter_KeyDownHandler();
                    break;

                case Key.Escape:
                    settingsWindow.Close();
                    break;
            }
        }

        public async Task<string?> UserInput()
        {
            var window = new ServiceWindow()
            {
                //DataContext = _serviceProvider.GetService<>
            };

            await OpenWindowWithDimmer(window, SettingsWindow.Instance, SettingsWindow.Workspace);

            return window.SelectedFilePath;
        }

        public async Task About()
        {
            await OpenWindowWithDimmer(new AboutWindow(), MainWindow.Instance, MainWindow.Workspace);
        }

        public async Task ModbusScanner()
        {
            await OpenWindowWithDimmer(new ModbusScannerWindow(), MainWindow.Instance, MainWindow.Workspace);
        }

        private async Task OpenWindowWithDimmer(Window window, Window? owner, Visual? workspace)
        {
            if (owner == null)
            {
                throw new Exception("Не задан владелец окна.");
            }

            if (workspace == null)
            {
                await window.ShowDialog(owner);
                return;
            }

            await Dispatcher.UIThread.Invoke(async () =>
            {
                double workspaceOpacity_Default = workspace.Opacity;

                workspace.Opacity = WorkspaceOpacity_OpenChildWindow;

                await window.ShowDialog(owner);

                workspace.Opacity = workspaceOpacity_Default;
            });
        }
    }
}
