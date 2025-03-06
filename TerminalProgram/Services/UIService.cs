using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;
using TerminalProgramBase.Views;
using Services.Interfaces;

namespace TerminalProgramBase.Services
{
    public class UIService : IUIService
    {
        public async Task RunInUIThread(Action runnedAction)
        {
            await Dispatcher.UIThread.InvokeAsync(runnedAction);
        }

        public async Task CopyToClipboard(string data)
        {
            var clipboard = TopLevel.GetTopLevel(MainWindow.Instance)?.Clipboard;
            var dataObject = new DataObject();

            dataObject.Set(DataFormats.Text, data);

            if (clipboard != null)
            {
                await clipboard.SetDataObjectAsync(dataObject);
            }
        }

        public Version? GetAppVersion()
        {
            return AboutWindow.GetAppVersion();
        }

        public void Set_Dark_Theme()
        {
            if (Application.Current != null)
            {
                Application.Current.Resources.MergedDictionaries.Clear();

                Application.Current.Resources.MergedDictionaries.Add(new ResourceInclude(
                    new Uri("avares://AppDesign/Themes/Dark.axaml"))
                {
                    Source = new Uri("avares://AppDesign/Themes/Dark.axaml")
                });

                Application.Current.RequestedThemeVariant =
                    new Avalonia.Styling.ThemeVariant("Dark", Application.Current.ActualThemeVariant);
            }
        }

        public void Set_Light_Theme()
        {
            if (Application.Current != null)
            {
                Application.Current.Resources.MergedDictionaries.Clear();

                Application.Current.Resources.MergedDictionaries.Add(new ResourceInclude(
                    new Uri("avares://AppDesign/Themes/Light.axaml"))
                {
                    Source = new Uri("avares://AppDesign/Themes/Light.axaml")
                });

                Application.Current.RequestedThemeVariant =
                    new Avalonia.Styling.ThemeVariant("Light", Application.Current.ActualThemeVariant);
            }
        }
    }
}
