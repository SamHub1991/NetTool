using System.Windows;
using System.Windows.Controls;
using DeerFlow.WPF.ViewModels;

namespace DeerFlow.WPF.Views.Windows;

/// <summary>
/// 独立任务窗口 — 纯 UI 编排，业务逻辑在 TaskWindowViewModel 中
/// Code-behind 只处理 WPF 特有的 UI 行为（自动滚动）
/// </summary>
public partial class TaskWindow : Window
{
    private bool _autoScroll = true;

    public TaskWindow()
    {
        InitializeComponent();

        // 设置 DataContext
        DataContext = new TaskWindowViewModel();
    }

    /// <summary>
    /// 输出内容滚动时：用户手动滚动到顶部则停止自动滚动，滚到底部则恢复
    /// </summary>
    private void OnOutputScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // 如果用户手动滚到了顶部，暂停自动滚动
        if (e.VerticalOffset < e.ExtentHeight - e.ViewportHeight - 10)
        {
            _autoScroll = false;
            return;
        }

        // 如果滚到底部，恢复自动滚动
        if (e.VerticalOffset >= e.ExtentHeight - e.ViewportHeight - 2)
        {
            _autoScroll = true;
        }

        // 自动滚动到底部
        if (_autoScroll && e.VerticalChange != 0 && sender is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToBottom();
        }
    }
}
