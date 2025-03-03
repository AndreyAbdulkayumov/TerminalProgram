namespace ViewModels.Macros.DataTypes
{
    public class MacrosData
    {
        public readonly string Name;
        public readonly Action MacrosAction;

        public MacrosData(string name, Action macrosAction)
        {
            Name = name;
            MacrosAction = macrosAction;
        }
    }
}
