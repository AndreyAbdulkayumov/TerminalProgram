using Avalonia.Controls;
using Avalonia.Threading;
using Services.Interfaces;
using System;
using System.Threading.Tasks;
using TerminalProgramBase.Views;

namespace TerminalProgramBase.Services
{
    public class OpenChildWindow : IOpenChildWindow
    {
        private const double WorkspaceOpacity_OpenChildWindow = 0.15;

        public async Task About()
        {
            await OpenWindowWithDimmer(new AboutWindow(), MainWindow.Instance, MainWindow.Workspace);
        }

        public async Task ModbusScanner()
        {
            await OpenWindowWithDimmer(new ModbusScannerWindow(), MainWindow.Instance, MainWindow.Workspace);
        }

        private async Task OpenWindowWithDimmer(Window window, Window? owner, Grid? workspace)
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
