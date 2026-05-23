namespace SystemToolkit.Core.Models;

public class DiskInfo
{
    public string Name { get; set; } = string.Empty;
    public string? VolumeLabel { get; set; }
    public string DriveType { get; set; } = string.Empty;
    public string? DriveFormat { get; set; }
    public long TotalSize { get; set; }
    public long TotalFreeSpace { get; set; }
    public long UsedSpace => TotalSize - TotalFreeSpace;
    public float UsagePercentage => TotalSize > 0 ? (float)(UsedSpace * 100.0 / TotalSize) : 0;
}

public class DirectoryInfo
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public int FileCount { get; set; }
    public int DirectoryCount { get; set; }
    public DateTime LastModified { get; set; }
}

public class LargeFile
{
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime ModifiedTime { get; set; }
    public string Extension { get; set; } = string.Empty;
}

public class SpaceCleanupSuggestion
{
    public string Category { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public bool SafeToDelete { get; set; }
    public string Description { get; set; } = string.Empty;
}
