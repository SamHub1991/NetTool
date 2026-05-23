using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeerFlow.WPF.Models;

/// <summary>
/// 技能模型，实现 INotifyPropertyChanged 以支持安装状态实时更新
/// </summary>
public class SkillItem : INotifyPropertyChanged
{
    /// <summary>技能唯一标识</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    private string _name = string.Empty;
    /// <summary>技能名称</summary>
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private string _description = string.Empty;
    /// <summary>技能描述</summary>
    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    private string _content = string.Empty;
    /// <summary>SKILL.md 文件内容</summary>
    public string Content
    {
        get => _content;
        set { _content = value; OnPropertyChanged(); }
    }

    private bool _isInstalled;
    /// <summary>是否已安装</summary>
    public bool IsInstalled
    {
        get => _isInstalled;
        set { _isInstalled = value; OnPropertyChanged(); }
    }

    private string _version = "1.0.0";
    /// <summary>版本号</summary>
    public string Version
    {
        get => _version;
        set { _version = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
