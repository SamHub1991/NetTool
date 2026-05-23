namespace SystemToolkit.Core.Models;

using System.ServiceProcess;

public class ServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ServiceControllerStatus Status { get; set; }
    public ServiceStartMode StartMode { get; set; }
    public string? ServiceType { get; set; }
}

public enum ServiceStartMode
{
    Boot = 0,
    System = 1,
    Automatic = 2,
    Manual = 3,
    Disabled = 4
}
