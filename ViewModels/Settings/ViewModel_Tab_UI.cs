using Core.Models.Settings;
using MessageBox_Core;
using ReactiveUI;
using System.Reactive;

namespace ViewModels.Settings
{
    public class ViewModel_Tab_UI : ReactiveObject
    {
        private ReactiveCommand<Unit, Unit> Select_Dark_Theme { get; }
        private ReactiveCommand<Unit, Unit> Select_Light_Theme { get; }

        private readonly Model_Settings SettingsFile;


        public ViewModel_Tab_UI(Action Set_Dark_Theme_Handler, Action Set_Light_Theme_Handler, Action<string, MessageType> Message)
        {
            SettingsFile = Model_Settings.Model;

            Select_Dark_Theme = ReactiveCommand.Create(() =>
            {
                Set_Dark_Theme_Handler();
                SettingsFile.AppData.ThemeName = AppInfo.ThemeName_Dark;
                SettingsFile.SaveAppInfo(SettingsFile.AppData);
            });
            Select_Dark_Theme.ThrownExceptions.Subscribe(error => Message.Invoke("Не удалось корректно переключиться на темную тему.\n\n" + error.Message, MessageType.Error));

            Select_Light_Theme = ReactiveCommand.Create(() =>
            {
                Set_Light_Theme_Handler();
                SettingsFile.AppData.ThemeName = AppInfo.ThemeName_Light;
                SettingsFile.SaveAppInfo(SettingsFile.AppData);
            });
            Select_Light_Theme.ThrownExceptions.Subscribe(error => Message.Invoke("Не удалось корректно переключиться на светлую тему.\n\n" + error.Message, MessageType.Error));
        }
    }
}
