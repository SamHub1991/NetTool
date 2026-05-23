namespace SystemToolkit.Core.Models;

public class CodeTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public List<TemplateParameter> Parameters { get; set; } = new();
}

public class TemplateParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public string? Description { get; set; }
}

public class FormatResult
{
    public string? FormattedText { get; set; }
    public string? Error { get; set; }
    public bool Success => Error == null;
}

public class RegexTestResult
{
    public bool IsMatch { get; set; }
    public List<MatchInfo> Matches { get; set; } = new();
    public long ExecutionTimeMs { get; set; }
}

public class MatchInfo
{
    public int Index { get; set; }
    public int Length { get; set; }
    public string Value { get; set; } = string.Empty;
    public Dictionary<string, string> Groups { get; set; } = new();
}
