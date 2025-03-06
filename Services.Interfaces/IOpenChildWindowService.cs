namespace Services.Interfaces
{
    public interface IOpenChildWindowService
    {
        Task Settings();
        Task<string?> UserInput();
        Task About();
        Task ModbusScanner();
    }
}
