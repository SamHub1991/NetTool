namespace SystemToolkit.Core.Models;

public class ClipboardHistoryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int Size { get; set; }
}

public class ScreenshotResult
{
    public string ImagePath { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime Timestamp { get; set; }
    public string? OcrText { get; set; }
}

public class TimerTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public DateTime TriggerTime { get; set; }
    public TimeSpan Duration { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsActive { get; set; }
    public string? NotificationMessage { get; set; }
}

public class ShortcutMapping
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TriggerKeys { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? ActionData { get; set; }
    public bool IsActive { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class MacroRecording
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<MacroStep> Steps { get; set; } = new();
    public DateTime CreatedTime { get; set; }
}

public class MacroStep
{
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public int Delay { get; set; }
}
