using DeerFlow.WPF.Core;
using Xunit;

namespace DeerFlow.WPF.Tests.Core;

public class ViewModelBaseTests
{
    [Fact]
    public void SetProperty_SameValue_ReturnsFalse()
    {
        var vm = new TestViewModel { Name = "Test" };
        var result = vm.SetNameExposed("Test");
        Assert.False(result);
    }

    [Fact]
    public void SetProperty_DifferentValue_ReturnsTrue()
    {
        var vm = new TestViewModel { Name = "Old" };
        var result = vm.SetNameExposed("New");
        Assert.True(result);
    }

    [Fact]
    public void SetProperty_RaisesPropertyChanged()
    {
        var vm = new TestViewModel();
        var raised = false;

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TestViewModel.Name))
                raised = true;
        };

        vm.Name = "Changed";
        Assert.True(raised);
    }

    [Fact]
    public void SetProperty_NoPropertyChanged_WhenSameValue()
    {
        var vm = new TestViewModel { Name = "Same" };
        var raised = false;

        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TestViewModel.Name))
                raised = true;
        };

        vm.Name = "Same";
        Assert.False(raised);
    }
}

/// <summary>
/// 用于测试 ViewModelBase 的帮助类
/// </summary>
public class TestViewModel : ViewModelBase
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// 暴露 SetProperty 返回值用于测试
    /// </summary>
    public bool SetNameExposed(string value) => SetProperty(ref _name, value, nameof(Name));
}
