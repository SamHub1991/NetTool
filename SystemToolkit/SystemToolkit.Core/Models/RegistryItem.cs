namespace SystemToolkit.Core.Models;

public class RegistryItem
{
    public string KeyPath { get; set; } = string.Empty;
    public string ValueName { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string ValueType { get; set; } = string.Empty;
    public DateTime? ModifiedTime { get; set; }
}

public class RegistryBackup
{
    public string BackupPath { get; set; } = string.Empty;
    public DateTime BackupTime { get; set; }
    public string Description { get; set; } = string.Empty;
}
