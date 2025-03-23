namespace ViewModels.Macros.DataTypes
{
    public class EditCommandParameters
    {
        public readonly string? CommandName;
        public readonly object? InitData;

        public EditCommandParameters(string? commandName, object? initData)
        {
            CommandName = commandName;
            InitData = initData;
        }
    }
}
