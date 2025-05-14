namespace ViewModels.Macros.DataTypes;

internal interface ICommandContent
{
    string? Name { get; set; }
    Guid Id { get; }
    object GetContent();
}
