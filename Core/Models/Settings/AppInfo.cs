namespace Core.Models.Settings
{
    public enum AppTheme
    {
        Dark,
        Light
    }

    public enum AppMode
    {
        NoProtocol,
        ModbusClient,
        ModbusTCPServer
    }

    public class AppInfo
    {
        public string? SelectedPresetFileName { get; set; }
        public AppTheme ThemeName { get; set; }
        public AppMode SelectedMode { get; set; }


        public static AppInfo GetDefault(string DefaultPresetName)
        {
            return new AppInfo()
            {
                SelectedPresetFileName = DefaultPresetName,
                ThemeName = AppTheme.Dark,
                SelectedMode = AppMode.ModbusClient
            };
        }
    }
}
