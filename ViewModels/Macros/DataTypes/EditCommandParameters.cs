namespace ViewModels.Macros.DataTypes
{
    public class EditCommandParameters
    {
        public readonly string? CommandName;
        public readonly object? InitData;
        public readonly IEnumerable<string?>? ExistingCommandNames;

        public EditCommandParameters(string? commandName, object? initData, IEnumerable<string?>? existingCommandNames)
        {
            CommandName = commandName;
            InitData = initData;
            ExistingCommandNames = existingCommandNames;
        }
    }
}
