using Xunit;
namespace Philiprehberger.FeatureFlag.Tests;

public class FeatureFlagsTests
{
    [Fact]
    public void IsEnabled_WithEnabledFlag_ReturnsTrue()
    {
        var flags = FeatureFlags.ForTesting(("dark-mode", true));

        Assert.True(flags.IsEnabled("dark-mode"));
    }

    [Fact]
    public void IsEnabled_WithDisabledFlag_ReturnsFalse()
    {
        var flags = FeatureFlags.ForTesting(("dark-mode", false));

        Assert.False(flags.IsEnabled("dark-mode"));
    }

    [Fact]
    public void IsEnabled_WithUnknownFlag_ReturnsFalse()
    {
        var flags = FeatureFlags.ForTesting(("dark-mode", true));

        Assert.False(flags.IsEnabled("unknown"));
    }

    [Fact]
    public void IsEnabled_IsCaseInsensitive()
    {
        var flags = FeatureFlags.ForTesting(("Dark-Mode", true));

        Assert.True(flags.IsEnabled("dark-mode"));
        Assert.True(flags.IsEnabled("DARK-MODE"));
    }

    [Fact]
    public void IsEnabled_WithAllowedUser_ReturnsTrueRegardlessOfPercentage()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["feature"] = new(Enabled: true, Percentage: 0,
                    AllowedUsers: new HashSet<string> { "admin-1" })
            }
        };
        var flags = new FeatureFlags(options);

        Assert.True(flags.IsEnabled("feature", "admin-1"));
    }

    [Fact]
    public void IsEnabled_WithRoles_MatchesAllowedRole()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["admin-panel"] = new(Enabled: true,
                    AllowedRoles: new HashSet<string> { "admin" })
            }
        };
        var flags = new FeatureFlags(options);

        Assert.True(flags.IsEnabled("admin-panel", "user-1", new[] { "admin" }));
        Assert.False(flags.IsEnabled("admin-panel", "user-2", new[] { "viewer" }));
    }

    [Fact]
    public void IsEnabled_WithRoles_NoRolesProvided_ReturnsFalse()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["admin-panel"] = new(Enabled: true,
                    AllowedRoles: new HashSet<string> { "admin" })
            }
        };
        var flags = new FeatureFlags(options);

        Assert.False(flags.IsEnabled("admin-panel", "user-1", roles: null));
        Assert.False(flags.IsEnabled("admin-panel", "user-1", roles: Array.Empty<string>()));
    }

    [Fact]
    public void IsEnabled_WithContext_DelegatesToUserOverload()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["feature"] = new(Enabled: true,
                    AllowedRoles: new HashSet<string> { "beta" })
            }
        };
        var flags = new FeatureFlags(options);
        var context = new FeatureFlagContext
        {
            UserId = "user-1",
            Roles = new[] { "beta" }
        };

        Assert.True(flags.IsEnabled("feature", context));
    }

    [Fact]
    public void IsEnabled_WithContextNoUser_DelegatesToSimpleOverload()
    {
        var flags = FeatureFlags.ForTesting(("feature", true));
        var context = new FeatureFlagContext();

        Assert.True(flags.IsEnabled("feature", context));
    }

    [Fact]
    public void IsEnabled_WithPercentage_IsDeterministic()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["feature"] = new(Enabled: true, Percentage: 50)
            }
        };
        var flags = new FeatureFlags(options);

        var first = flags.IsEnabled("feature", "user-42");
        var second = flags.IsEnabled("feature", "user-42");

        Assert.Equal(first, second);
    }

    [Fact]
    public void GetVariant_ReturnsConsistentVariant()
    {
        var flags = FeatureFlags.ForTesting(("experiment", true));
        var variants = new[] { "control", "variant-a", "variant-b" };

        var first = flags.GetVariant("experiment", "user-1", variants);
        var second = flags.GetVariant("experiment", "user-1", variants);

        Assert.NotNull(first);
        Assert.Equal(first, second);
        Assert.Contains(first, variants);
    }

    [Fact]
    public void GetVariant_WithEmptyVariants_ReturnsNull()
    {
        var flags = FeatureFlags.ForTesting(("experiment", true));

        Assert.Null(flags.GetVariant("experiment", "user-1", Array.Empty<string>()));
    }

    [Fact]
    public void ForTesting_CreatesWorkingInstance()
    {
        var flags = FeatureFlags.ForTesting(
            ("a", true),
            ("b", false));

        Assert.True(flags.IsEnabled("a"));
        Assert.False(flags.IsEnabled("b"));
    }
}
