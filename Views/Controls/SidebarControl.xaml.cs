using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DeerFlow.WPF.ViewModels;

namespace DeerFlow.WPF.Views.Controls;

/// <summary>
/// 侧边栏导航控件 — 纯 UI 编排
/// 导航事件由 PreviewMouseDown 触发 SelectionCommand（支持重复点击同一项）
/// 选中高亮由 ListBoxItem.IsSelected 管理
/// </summary>
public partial class SidebarControl : UserControl
{
    public SidebarControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 鼠标按下时，沿可视树向上查找点击的 ListBoxItem
    /// 执行导航命令并更新选中高亮
    /// </summary>
    private void OnMenuPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;

        // 从点击的源元素沿可视树向上找到 ListBoxItem
        var listBoxItem = FindVisualParent<ListBoxItem>(e.OriginalSource as DependencyObject);
        if (listBoxItem?.DataContext is MenuItemViewModel menuItem)
        {
            menuItem.SelectionCommand.Execute(null);
            listBoxItem.IsSelected = true;
            e.Handled = true;
        }
    }

    /// <summary>
    /// 沿可视树向上查找指定类型 T 的祖先元素
    /// </summary>
    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T parent) return parent;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }
}
