namespace Core.Models.Settings
{
    public class AppInfo
    {
        public string? SelectedPresetFileName { get; set; }

        public const string ThemeName_Dark = "Dark";
        public const string ThemeName_Light = "Light";

        public string? ThemeName { get; set; }


        public static AppInfo GetDefault(string DefaultPresetName)
        {
            return new AppInfo()
            {
                SelectedPresetFileName = DefaultPresetName,
                ThemeName = ThemeName_Dark
            };
        }
    }
}
