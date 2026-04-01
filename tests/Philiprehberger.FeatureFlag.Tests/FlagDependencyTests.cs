using Xunit;
namespace Philiprehberger.FeatureFlag.Tests;

public class FlagDependencyTests
{
    [Fact]
    public void IsEnabled_WithSatisfiedDependency_ReturnsTrue()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["parent"] = new(Enabled: true),
                ["child"] = new(Enabled: true, DependsOn: "parent")
            }
        };
        var flags = new FeatureFlags(options);

        Assert.True(flags.IsEnabled("child"));
    }

    [Fact]
    public void IsEnabled_WithDisabledDependency_ReturnsFalse()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["parent"] = new(Enabled: false),
                ["child"] = new(Enabled: true, DependsOn: "parent")
            }
        };
        var flags = new FeatureFlags(options);

        Assert.False(flags.IsEnabled("child"));
    }

    [Fact]
    public void IsEnabled_WithMissingDependency_ReturnsFalse()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["child"] = new(Enabled: true, DependsOn: "nonexistent")
            }
        };
        var flags = new FeatureFlags(options);

        Assert.False(flags.IsEnabled("child"));
    }

    [Fact]
    public void IsEnabled_WithNoDependency_IgnoresCheck()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["standalone"] = new(Enabled: true)
            }
        };
        var flags = new FeatureFlags(options);

        Assert.True(flags.IsEnabled("standalone"));
    }

    [Fact]
    public void IsEnabled_WithChainedDependencies_ChecksEntireChain()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["grandparent"] = new(Enabled: true),
                ["parent"] = new(Enabled: true, DependsOn: "grandparent"),
                ["child"] = new(Enabled: true, DependsOn: "parent")
            }
        };
        var flags = new FeatureFlags(options);

        Assert.True(flags.IsEnabled("child"));
    }

    [Fact]
    public void IsEnabled_WithBrokenChain_ReturnsFalse()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["grandparent"] = new(Enabled: false),
                ["parent"] = new(Enabled: true, DependsOn: "grandparent"),
                ["child"] = new(Enabled: true, DependsOn: "parent")
            }
        };
        var flags = new FeatureFlags(options);

        Assert.False(flags.IsEnabled("child"));
    }

    [Fact]
    public void IsEnabled_WithUser_RespectsDependency()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["parent"] = new(Enabled: false),
                ["child"] = new(Enabled: true, DependsOn: "parent")
            }
        };
        var flags = new FeatureFlags(options);

        Assert.False(flags.IsEnabled("child", "user-1"));
    }

    [Fact]
    public void IsEnabled_DependencyIsCaseInsensitive()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["Parent"] = new(Enabled: true),
                ["child"] = new(Enabled: true, DependsOn: "Parent")
            }
        };
        var flags = new FeatureFlags(options);

        Assert.True(flags.IsEnabled("child"));
    }
}
