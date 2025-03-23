namespace Services
{
    public interface IUIServices
    {
        Task RunInUIThread(Action runnedAction);
        Task OpenWindow_ModbusScanner();
        Task CopyToClipboard(string data);
        void Set_Dark_Theme();
        void Set_Light_Theme();
        Version? GetAppVersion();
    }
}
