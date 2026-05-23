using DeerFlow.WPF.Core;
using Xunit;

namespace DeerFlow.WPF.Tests.Core;

public class RelayCommandTests
{
    [Fact]
    public void Execute_CallsDelegate()
    {
        var called = false;
        var cmd = new RelayCommand(_ => called = true);

        cmd.Execute(null);
        Assert.True(called);
    }

    [Fact]
    public void CanExecute_ReturnsTrue_ByDefault()
    {
        var cmd = new RelayCommand(_ => { });
        Assert.True(cmd.CanExecute(null));
    }

    [Fact]
    public void CanExecute_ReturnsFalse_WhenPredicateReturnsFalse()
    {
        var cmd = new RelayCommand(_ => { }, _ => false);
        Assert.False(cmd.CanExecute(null));
    }

    [Fact]
    public void GenericRelayCommand_PassesTypedParameter()
    {
        string? received = null;
        var cmd = new RelayCommand<string>(s => received = s);

        cmd.Execute("Hello");
        Assert.Equal("Hello", received);
    }

    [Fact]
    public void GenericRelayCommand_CanExecute_WithTypedPredicate()
    {
        var cmd = new RelayCommand<int>(_ => { }, v => v > 0);

        Assert.True(cmd.CanExecute(5));
        Assert.False(cmd.CanExecute(0));
        Assert.False(cmd.CanExecute(-1));
    }
}
