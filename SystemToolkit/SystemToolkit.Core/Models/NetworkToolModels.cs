namespace SystemToolkit.Core.Models;

public class PingResult
{
    public string Host { get; set; } = string.Empty;
    public int TimeToLive { get; set; }
    public long RoundtripTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
}

public class PortScanResult
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsOpen { get; set; }
    public string? ServiceName { get; set; }
    public long ResponseTime { get; set; }
}

public class TracertHop
{
    public int HopNumber { get; set; }
    public string? Hostname { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public long RoundtripTime1 { get; set; }
    public long RoundtripTime2 { get; set; }
    public long RoundtripTime3 { get; set; }
}

public class HttpTestRequest
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? Body { get; set; }
    public string? ContentType { get; set; }
}

public class HttpTestResponse
{
    public int StatusCode { get; set; }
    public string StatusDescription { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public long ResponseTime { get; set; }
    public string? Error { get; set; }
}

public class PortMonitorInfo
{
    public int Port { get; set; }
    public string? ProcessName { get; set; }
    public int ProcessId { get; set; }
    public string LocalAddress { get; set; } = string.Empty;
    public string RemoteAddress { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
