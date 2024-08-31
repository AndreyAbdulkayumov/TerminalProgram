using Core.Models.Settings;
using MessageBox_Core;
using ReactiveUI;
using System.Reactive;

namespace ViewModels.Settings
{
    public class Tab_UI_VM : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> Select_Dark_Theme { get; }
        public ReactiveCommand<Unit, Unit> Select_Light_Theme { get; }

        private readonly Model_Settings SettingsFile;


        public Tab_UI_VM(Action set_Dark_Theme_Handler, Action set_Light_Theme_Handler, Action<string, MessageType> message)
        {
            SettingsFile = Model_Settings.Model;

            Select_Dark_Theme = ReactiveCommand.Create(() =>
            {
                set_Dark_Theme_Handler();

                SettingsFile.AppData.ThemeName = AppTheme.Dark;
            });
            Select_Dark_Theme.ThrownExceptions.Subscribe(error => message.Invoke("Не удалось корректно переключиться на темную тему.\n\n" + error.Message, MessageType.Error));

            Select_Light_Theme = ReactiveCommand.Create(() =>
            {
                set_Light_Theme_Handler();

                SettingsFile.AppData.ThemeName = AppTheme.Light;
            });
            Select_Light_Theme.ThrownExceptions.Subscribe(error => message.Invoke("Не удалось корректно переключиться на светлую тему.\n\n" + error.Message, MessageType.Error));
        }
    }
}
