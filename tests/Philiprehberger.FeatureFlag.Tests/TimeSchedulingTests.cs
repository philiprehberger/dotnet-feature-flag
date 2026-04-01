using Xunit;
namespace Philiprehberger.FeatureFlag.Tests;

public class TimeSchedulingTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private static FeatureFlags CreateFlags(FeatureFlagDefinition definition, DateTimeOffset? now = null)
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["feature"] = definition
            }
        };
        var tp = new FakeTimeProvider(now ?? Now);
        return new FeatureFlags(options, tp, new FeatureFlagAnalytics());
    }

    [Fact]
    public void IsEnabled_BeforeEnableFrom_ReturnsFalse()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: true,
            EnableFrom: Now.AddHours(1)));

        Assert.False(flags.IsEnabled("feature"));
    }

    [Fact]
    public void IsEnabled_AfterEnableFrom_ReturnsTrue()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: true,
            EnableFrom: Now.AddHours(-1)));

        Assert.True(flags.IsEnabled("feature"));
    }

    [Fact]
    public void IsEnabled_ExactlyAtEnableFrom_ReturnsTrue()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: true,
            EnableFrom: Now));

        Assert.True(flags.IsEnabled("feature"));
    }

    [Fact]
    public void IsEnabled_BeforeEnableUntil_ReturnsTrue()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: true,
            EnableUntil: Now.AddHours(1)));

        Assert.True(flags.IsEnabled("feature"));
    }

    [Fact]
    public void IsEnabled_AfterEnableUntil_ReturnsFalse()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: true,
            EnableUntil: Now.AddHours(-1)));

        Assert.False(flags.IsEnabled("feature"));
    }

    [Fact]
    public void IsEnabled_ExactlyAtEnableUntil_ReturnsFalse()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: true,
            EnableUntil: Now));

        Assert.False(flags.IsEnabled("feature"));
    }

    [Fact]
    public void IsEnabled_WithinWindow_ReturnsTrue()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: true,
            EnableFrom: Now.AddHours(-1),
            EnableUntil: Now.AddHours(1)));

        Assert.True(flags.IsEnabled("feature"));
    }

    [Fact]
    public void IsEnabled_OutsideWindow_ReturnsFalse()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: true,
            EnableFrom: Now.AddHours(1),
            EnableUntil: Now.AddHours(2)));

        Assert.False(flags.IsEnabled("feature"));
    }

    [Fact]
    public void IsEnabled_WithUser_RespectsSchedule()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: true,
            EnableFrom: Now.AddHours(1)));

        Assert.False(flags.IsEnabled("feature", "user-1"));
    }

    [Fact]
    public void IsEnabled_DisabledFlag_IgnoresSchedule()
    {
        var flags = CreateFlags(new FeatureFlagDefinition(
            Enabled: false,
            EnableFrom: Now.AddHours(-1),
            EnableUntil: Now.AddHours(1)));

        Assert.False(flags.IsEnabled("feature"));
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FakeTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
