using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MessageBox_Core;
using ViewModels.Settings;
using Avalonia.Platform.Storage;
using System.Linq;
using Avalonia.Threading;
using MessageBox_AvaloniaUI;


namespace TerminalProgramBase.Views.Settings
{
    public partial class SettingsWindow : Window
    {
        private readonly Settings_VM ViewModel;

        private readonly double WorkspaceOpacity_Default;
        private const double WorkspaceOpacity_OpenChildWindow = 0.15;

        private readonly IMessageBox Message;

        public SettingsWindow(Action set_Dark_Theme, Action set_Light_Theme)
        {
            InitializeComponent();

            WorkspaceOpacity_Default = Border_Workspace.Opacity;

            Message = new MessageBox(this);

            ViewModel = new Settings_VM(
                Message,
                Get_FilePath,
                Get_NewFileName,
                set_Dark_Theme,
                set_Light_Theme
                );

            DataContext = ViewModel;
        }

        private async void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            await ViewModel.Command_Loaded.Execute();
        }

        private async void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    await ViewModel.Command_File_Save.Execute();
                    break;

                case Key.Escape:
                    Close();
                    break;
            }
        }

        private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        private void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task<string?> Get_FilePath(string WindowTitle)
        {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            TopLevel? topLevel = TopLevel.GetTopLevel(this);

            if (topLevel != null)
            {
                // Start async operation to open the dialog.
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = WindowTitle,
                    FileTypeFilter = [new FilePickerFileType("Файл настроек") { Patterns = ["*.json"] }],
                    AllowMultiple = false
                });

                if (files.Count >= 1)
                {
                    return files.First().Path.AbsolutePath;
                }
            }            

            return null;
        }

        private async Task<string?> Get_NewFileName()
        {
            var window = new ServiceWindow();

            await OpenWindowWithDimmer(async () =>
            {
                await window.ShowDialog(this);
            });            

            return window.SelectedFilePath;
        }

        /********************************************************/
        //
        //  Служебный функционал
        //
        /********************************************************/

        private async Task OpenWindowWithDimmer(Func<Task> OpenAction)
        {
            await Dispatcher.UIThread.Invoke(async () =>
            {
                Border_Workspace.Opacity = WorkspaceOpacity_OpenChildWindow;

                await OpenAction();

                Border_Workspace.Opacity = WorkspaceOpacity_Default;
            });
        }
    }
}
