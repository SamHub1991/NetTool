using System.Windows;
using System.Windows.Controls;
using DeerFlow.WPF.ViewModels;

namespace DeerFlow.WPF.Views.Pages;

/// <summary>
/// 设置页面代码后端，处理 PasswordBox 同步和初始化加载
/// </summary>
public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// 页面加载后同步 ViewModel 中的 ApiKey 到 PasswordBox
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && !string.IsNullOrEmpty(vm.ApiKey))
        {
            ApiKeyPasswordBox.Password = vm.ApiKey;
        }
    }

    /// <summary>
    /// PasswordBox 密码变更时同步到 ViewModel
    /// </summary>
    private void OnApiKeyPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb && DataContext is SettingsViewModel vm)
        {
            vm.ApiKey = pb.Password;
        }
    }
}
