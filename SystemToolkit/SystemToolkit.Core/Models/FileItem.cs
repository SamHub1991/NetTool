namespace SystemToolkit.Core.Models;

public class FileItem
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDirectory { get; set; }
    public string? Hash { get; set; }
}

public class RenameRule
{
    public string SearchPattern { get; set; } = string.Empty;
    public string ReplacePattern { get; set; } = string.Empty;
    public bool UseRegex { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
}

public class DuplicateFile
{
    public string Hash { get; set; } = string.Empty;
    public List<string> Paths { get; set; } = new();
    public long Size { get; set; }
}
