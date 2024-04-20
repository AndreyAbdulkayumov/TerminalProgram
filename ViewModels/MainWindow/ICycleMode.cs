namespace ViewModels.MainWindow
{
    internal interface ICycleMode
    {
        event EventHandler<EventArgs>? DeviceIsDisconnected;

        void SourceWindowClosingAction();

        void Start_Stop_Handler();
    }
}
