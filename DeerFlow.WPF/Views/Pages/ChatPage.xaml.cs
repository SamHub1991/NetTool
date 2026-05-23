using System.Windows.Controls;
using DeerFlow.WPF.ViewModels;

namespace DeerFlow.WPF.Views.Pages;

/// <summary>
/// 聊天页面 — 纯 UI 编排
/// 键盘快捷键由 InputBindings 绑定到命令处理
/// Code-behind 只处理 WPF 特有的 UI 行为（消息列表自动滚动）
/// </summary>
public partial class ChatPage : UserControl
{
    public ChatPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// 页面加载后订阅消息集合变更事件以实现自动滚动
    /// </summary>
    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ChatViewModel vm)
        {
            vm.Messages.CollectionChanged += (_, _) => ScrollToBottom();
        }
    }

    /// <summary>
    /// 滚动消息列表到底部
    /// </summary>
    private void ScrollToBottom()
    {
        if (MessageList.Items.Count > 0)
        {
            MessageList.ScrollIntoView(MessageList.Items[MessageList.Items.Count - 1]);
        }
    }
}
