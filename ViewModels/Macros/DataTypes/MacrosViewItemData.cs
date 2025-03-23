namespace ViewModels.Macros.DataTypes
{
    public class MacrosViewItemData
    {
        public readonly string Name;
        public readonly Action MacrosAction;

        public MacrosViewItemData(string name, Action macrosAction)
        {
            Name = name;
            MacrosAction = macrosAction;
        }
    }
}
