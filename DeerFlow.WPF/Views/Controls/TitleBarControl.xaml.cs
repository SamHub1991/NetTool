using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeerFlow.WPF.Views.Controls;

/// <summary>
/// 自定义标题栏控件，支持窗口拖拽（双击最大化/还原）和窗口控制按钮
/// </summary>
public partial class TitleBarControl : UserControl
{
    public TitleBarControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// 加载完成后订阅窗口状态变更，更新最大化按钮图标
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);
        if (window != null)
        {
            window.StateChanged += OnWindowStateChanged;
            UpdateMaximizeButton(window.WindowState);
        }
    }

    /// <summary>
    /// 窗口状态变更时更新最大化/还原按钮图标
    /// </summary>
    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            UpdateMaximizeButton(window.WindowState);
        }
    }

    /// <summary>
    /// 根据窗口状态更新最大化按钮图标
    /// </summary>
    private void UpdateMaximizeButton(WindowState state)
    {
        MaximizeButton.Content = state == WindowState.Maximized ? "❐" : "□";
        MaximizeButton.ToolTip = state == WindowState.Maximized ? "还原" : "最大化";
    }

    /// <summary>
    /// 标题栏鼠标按下：实现窗口拖拽，双击切换最大化/还原
    /// </summary>
    private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;

        var window = Window.GetWindow(this);
        if (window == null) return;

        // 双击标题栏切换最大化/还原
        if (e.ClickCount == 2)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
            return;
        }

        // 拖拽窗口
        window.DragMove();
    }
}
