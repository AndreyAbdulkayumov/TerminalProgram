namespace Services.Interfaces
{
    public interface IFileSystemService
    {
        Task<string?> Get_FilePath(string windowTitle, string pickerFileType, IReadOnlyList<string> patterns);
        void OpenUserManual();
    }
}
