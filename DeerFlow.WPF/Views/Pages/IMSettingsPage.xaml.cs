using System.Windows;
using System.Windows.Controls;
using DeerFlow.WPF.ViewModels;

namespace DeerFlow.WPF.Views.Pages;

/// <summary>
/// IM 配置页面代码后端，处理 PasswordBox 同步和初始化加载
/// </summary>
public partial class IMSettingsPage : UserControl
{
    public IMSettingsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// 页面加载后同步 ViewModel 中的 Token 到 PasswordBox
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is IMSettingsViewModel vm && !string.IsNullOrEmpty(vm.EditingToken))
        {
            TokenPasswordBox.Password = vm.EditingToken;
        }
    }

    /// <summary>
    /// PasswordBox 密码变更时同步到 ViewModel
    /// </summary>
    private void OnTokenPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb && DataContext is IMSettingsViewModel vm)
        {
            vm.EditingToken = pb.Password;
        }
    }
}
