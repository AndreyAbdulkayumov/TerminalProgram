using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Styling;
using MessageBox_Core;
using MessageBox_AvaloniaUI;
using ViewModels.Settings;
using Avalonia.Platform.Storage;
using System.Linq;


namespace TerminalProgram.Views.Settings
{
    public partial class SettingsWindow : Window
    {
        private readonly ViewModel_Settings ViewModel;

        private readonly IMessageBox Message;

        public SettingsWindow()
        {
            InitializeComponent();

            Message = new MessageBox(MainWindow.Instance, "Терминальная программа");

            ViewModel = new ViewModel_Settings(
                Message.Show,
                Message.ShowYesNoDialog,
                Get_FilePath,
                Get_NewFileName,
                Set_Dark_Theme,
                Set_Light_Theme
                );

            DataContext = ViewModel;
        }

        private async void Window_Loaded(object? sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.Command_Loaded.Execute();
            }
        }

        private async void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    await ViewModel.Command_File_Save.Execute();
                    break;

                case Key.Escape:
                    this.Close();
                    break;
            }
        }

        private void Chrome_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            this.BeginMoveDrag(e);
        }

        private void Button_Close_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
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
            ServiceWindow window = new ServiceWindow();

            await window.ShowDialog(this);

            return window.SelectedFilePath;
        }

        private void Set_Dark_Theme()
        {
            if (Application.Current != null)
            {
                Application.Current.Resources.MergedDictionaries.Clear();

                Application.Current.Resources.MergedDictionaries.Add(new ResourceInclude(
                    new Uri("avares://TerminalProgram/Themes/Dark.axaml"))
                {
                    Source = new Uri("avares://TerminalProgram/Themes/Dark.axaml")
                });

                Application.Current.RequestedThemeVariant =
                    new Avalonia.Styling.ThemeVariant("Dark", Application.Current.ActualThemeVariant);
            }
        }

        private void Set_Light_Theme()
        {
            if (Application.Current != null)
            {
                Application.Current.Resources.MergedDictionaries.Clear();

                Application.Current.Resources.MergedDictionaries.Add(new ResourceInclude(
                    new Uri("avares://TerminalProgram/Themes/Light.axaml"))
                {
                    Source = new Uri("avares://TerminalProgram/Themes/Light.axaml")
                });

                Application.Current.RequestedThemeVariant =
                    new Avalonia.Styling.ThemeVariant("Light", Application.Current.ActualThemeVariant);
            }
        }
    }
}
