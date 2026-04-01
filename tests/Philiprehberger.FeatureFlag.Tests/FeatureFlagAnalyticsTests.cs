using Xunit;
namespace Philiprehberger.FeatureFlag.Tests;

public class FeatureFlagAnalyticsTests
{
    [Fact]
    public void GetEvaluationCount_WithNoRecords_ReturnsZero()
    {
        var analytics = new FeatureFlagAnalytics();

        Assert.Equal(0, analytics.GetEvaluationCount("feature"));
    }

    [Fact]
    public void GetEvaluationCount_TracksEnabledAndDisabled()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("feature", enabled: true);
        analytics.Record("feature", enabled: false);
        analytics.Record("feature", enabled: true);

        Assert.Equal(3, analytics.GetEvaluationCount("feature"));
    }

    [Fact]
    public void GetEnabledCount_ReturnsOnlyEnabledEvaluations()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("feature", enabled: true);
        analytics.Record("feature", enabled: false);
        analytics.Record("feature", enabled: true);

        Assert.Equal(2, analytics.GetEnabledCount("feature"));
    }

    [Fact]
    public void GetDisabledCount_ReturnsOnlyDisabledEvaluations()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("feature", enabled: true);
        analytics.Record("feature", enabled: false);
        analytics.Record("feature", enabled: false);

        Assert.Equal(2, analytics.GetDisabledCount("feature"));
    }

    [Fact]
    public void GetEnabledRatio_CalculatesCorrectly()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("feature", enabled: true);
        analytics.Record("feature", enabled: true);
        analytics.Record("feature", enabled: false);
        analytics.Record("feature", enabled: false);

        Assert.Equal(0.5, analytics.GetEnabledRatio("feature"));
    }

    [Fact]
    public void GetEnabledRatio_WithNoRecords_ReturnsZero()
    {
        var analytics = new FeatureFlagAnalytics();

        Assert.Equal(0.0, analytics.GetEnabledRatio("feature"));
    }

    [Fact]
    public void GetEnabledRatio_AllEnabled_ReturnsOne()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("feature", enabled: true);
        analytics.Record("feature", enabled: true);

        Assert.Equal(1.0, analytics.GetEnabledRatio("feature"));
    }

    [Fact]
    public void GetUniqueUserCount_TracksDistinctUsers()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("feature", enabled: true, userId: "user-1");
        analytics.Record("feature", enabled: true, userId: "user-2");
        analytics.Record("feature", enabled: false, userId: "user-1");

        Assert.Equal(2, analytics.GetUniqueUserCount("feature"));
    }

    [Fact]
    public void GetUniqueUserCount_WithNoUsers_ReturnsZero()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("feature", enabled: true);

        Assert.Equal(0, analytics.GetUniqueUserCount("feature"));
    }

    [Fact]
    public void GetUniqueUserCount_WithUnknownFlag_ReturnsZero()
    {
        var analytics = new FeatureFlagAnalytics();

        Assert.Equal(0, analytics.GetUniqueUserCount("unknown"));
    }

    [Fact]
    public void Reset_ClearsAllData()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("feature-a", enabled: true);
        analytics.Record("feature-b", enabled: false);
        analytics.Reset();

        Assert.Equal(0, analytics.GetEvaluationCount("feature-a"));
        Assert.Equal(0, analytics.GetEvaluationCount("feature-b"));
    }

    [Fact]
    public void Reset_ByName_ClearsOnlySpecifiedFlag()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("feature-a", enabled: true);
        analytics.Record("feature-b", enabled: true);
        analytics.Reset("feature-a");

        Assert.Equal(0, analytics.GetEvaluationCount("feature-a"));
        Assert.Equal(1, analytics.GetEvaluationCount("feature-b"));
    }

    [Fact]
    public void FeatureFlags_AutomaticallyRecordsAnalytics()
    {
        var flags = FeatureFlags.ForTesting(("feature", true), ("disabled", false));

        flags.IsEnabled("feature");
        flags.IsEnabled("feature");
        flags.IsEnabled("disabled");
        flags.IsEnabled("unknown");

        Assert.Equal(2, flags.Analytics.GetEnabledCount("feature"));
        Assert.Equal(1, flags.Analytics.GetDisabledCount("disabled"));
        Assert.Equal(1, flags.Analytics.GetDisabledCount("unknown"));
    }

    [Fact]
    public void FeatureFlags_RecordsUserAnalytics()
    {
        var options = new FeatureFlagOptions
        {
            Flags = new Dictionary<string, FeatureFlagDefinition>
            {
                ["feature"] = new(Enabled: true)
            }
        };
        var flags = new FeatureFlags(options);

        flags.IsEnabled("feature", "user-1");
        flags.IsEnabled("feature", "user-2");
        flags.IsEnabled("feature", "user-1");

        Assert.Equal(3, flags.Analytics.GetEvaluationCount("feature"));
        Assert.Equal(2, flags.Analytics.GetUniqueUserCount("feature"));
    }

    [Fact]
    public void IsolatesStatsBetweenFlags()
    {
        var analytics = new FeatureFlagAnalytics();

        analytics.Record("flag-a", enabled: true);
        analytics.Record("flag-b", enabled: false);

        Assert.Equal(1, analytics.GetEnabledCount("flag-a"));
        Assert.Equal(0, analytics.GetDisabledCount("flag-a"));
        Assert.Equal(0, analytics.GetEnabledCount("flag-b"));
        Assert.Equal(1, analytics.GetDisabledCount("flag-b"));
    }
}
