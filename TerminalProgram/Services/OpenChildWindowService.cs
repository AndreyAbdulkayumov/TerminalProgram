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
using TerminalProgramBase.Views.Macros;
using TerminalProgramBase.Views.Settings;
using ViewModels.Macros;
using ViewModels.Macros.MacrosEdit;
using ViewModels.Settings;

namespace TerminalProgramBase.Services
{
    public class OpenChildWindowService : IOpenChildWindowService
    {
        private Settings_VM? _settingsVM;
        private Macros_VM? _macrosVM;
        private EditMacros_VM? _editMacrosVM;

        private const double WorkspaceOpacity_OpenChildWindow = 0.15;

        private static bool _macrosWindowIsOpen = false;

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
            var window = new ServiceWindow();

            await OpenWindowWithDimmer(window, SettingsWindow.Instance, SettingsWindow.Workspace);

            return window.SelectedFilePath;
        }

        public async Task About()
        {
            await OpenWindowWithDimmer(new AboutWindow(), MainWindow.Instance, MainWindow.Workspace);
        }

        public async Task ModbusScanner()
        {
            var window = new ModbusScannerWindow();

            await OpenWindowWithDimmer(window, MainWindow.Instance, MainWindow.Workspace);
        }

        public void Macros()
        {
            try
            {
                if (_macrosWindowIsOpen)
                {
                    return;
                }

                if (MainWindow.Instance == null)
                {
                    throw new Exception("Не задан владелец окна.");
                }

                _macrosWindowIsOpen = true;

                _macrosVM = _serviceProvider.GetService<Macros_VM>();

                if (_macrosVM == null)
                {
                    return;
                }

                var window = new MacrosWindow()
                {
                    DataContext = _macrosVM
                };

                window.Closed += (object? sender, EventArgs e) =>
                {
                    _macrosWindowIsOpen = false;
                };

                window.Show(MainWindow.Instance);
            }

            catch (Exception error)
            {
                _macrosWindowIsOpen = false;

                throw new Exception(error.Message);
            }                                   
        }

        public async Task<object?> EditMacros(object? parameters)
        {
            _editMacrosVM = _serviceProvider.GetService<EditMacros_VM>();

            if (_editMacrosVM == null)
            {
                return null;
            }

            // Если макрос уже существует заполняем его данными
            if (parameters != null)
            {
                _editMacrosVM.SetParameters(parameters);
            }            

            var window = new EditMacrosWindow()
            {
                DataContext = _editMacrosVM
            };

            await OpenWindowWithDimmer(window, MacrosWindow.Instance, MacrosWindow.Workspace);

            return _editMacrosVM.Saved ? _editMacrosVM.GetMacrosContent() : null;
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
