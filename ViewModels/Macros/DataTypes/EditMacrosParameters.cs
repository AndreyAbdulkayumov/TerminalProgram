namespace ViewModels.Macros.DataTypes
{
    public class EditMacrosParameters
    {
        public readonly string? MacrosName;
        public readonly object? InitData;
        public readonly IEnumerable<string?>? ExistingMacrosNames;

        public EditMacrosParameters(string? macrosName, object? initData, IEnumerable<string?>? existingMacrosNames)
        {
            MacrosName = macrosName;
            InitData = initData;
            ExistingMacrosNames = existingMacrosNames;
        }
    }
}
