namespace ViewModels.Macros.MacrosItem
{
    public class MacrosData
    {
        public readonly string Name;
        public readonly Func<Task> Action;

        public MacrosData(string name, Func<Task> action)
        {
            Name = name;
            Action = action;
        }
    }
}
