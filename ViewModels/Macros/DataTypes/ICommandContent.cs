namespace ViewModels.Macros.DataTypes
{
    public interface ICommandContent
    {
        string? Name { get; set; }
        Guid Id { get; }
        object GetContent();
    }
}
