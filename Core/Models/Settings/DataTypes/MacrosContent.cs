namespace Core.Models.Settings.DataTypes
{
    public class MacrosContent<T>
    {
        public string? MacrosName { get; set; }
        public List<T>? Commands { get; set; }
    }
}
