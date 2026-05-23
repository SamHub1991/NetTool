namespace SystemToolkit.Core.Models;

public class HashResult
{
    public string Algorithm { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty;
}

public class EncryptionResult
{
    public string Algorithm { get; set; } = string.Empty;
    public string EncryptedData { get; set; } = string.Empty;
    public string? Key { get; set; }
    public string? IV { get; set; }
    public string? Error { get; set; }
    public bool Success => Error == null;
}

public class DecryptionResult
{
    public string Algorithm { get; set; } = string.Empty;
    public string DecryptedData { get; set; } = string.Empty;
    public string? Error { get; set; }
    public bool Success => Error == null;
}

public class CsvColumn
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Index { get; set; }
}

public class CsvData
{
    public List<CsvColumn> Columns { get; set; } = new();
    public List<Dictionary<string, string?>> Rows { get; set; } = new();
}

public class ExcelSheetData
{
    public string SheetName { get; set; } = string.Empty;
    public List<List<string?>> Data { get; set; } = new();
    public int RowCount => Data.Count;
    public int ColumnCount => Data.FirstOrDefault()?.Count ?? 0;
}

public class DbConnectionInfo
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseType { get; set; } = string.Empty;
    public string? Name { get; set; }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string? Exception { get; set; }
}

public class LogStatistics
{
    public int TotalEntries { get; set; }
    public Dictionary<string, int> LevelCounts { get; set; } = new();
    public DateTime? FirstEntry { get; set; }
    public DateTime? LastEntry { get; set; }
    public List<LogAlert> Alerts { get; set; } = new();
}

public class LogAlert
{
    public string Message { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int Count { get; set; }
    public DateTime FirstOccurrence { get; set; }
    public DateTime LastOccurrence { get; set; }
}
