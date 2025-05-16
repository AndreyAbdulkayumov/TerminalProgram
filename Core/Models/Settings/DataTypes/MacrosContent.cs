namespace Core.Models.Settings.DataTypes;

public class MacrosContent<T1, T2>
{
    public string? MacrosName { get; set; }
    public T1? AdditionalData { get; set; }
    public List<T2>? Commands { get; set; }
}
