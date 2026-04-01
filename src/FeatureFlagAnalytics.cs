using System.Collections.Concurrent;

namespace Philiprehberger.FeatureFlag;

/// <summary>
/// Tracks per-flag evaluation statistics including evaluation counts,
/// enabled/disabled ratios, and unique user counts.
/// </summary>
public sealed class FeatureFlagAnalytics
{
    private readonly ConcurrentDictionary<string, FlagStats> _stats = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Records a flag evaluation result for the given feature and optional user.
    /// </summary>
    /// <param name="featureName">The name of the evaluated feature flag.</param>
    /// <param name="enabled">Whether the evaluation resulted in the flag being enabled.</param>
    /// <param name="userId">Optional user identifier for unique user tracking.</param>
    public void Record(string featureName, bool enabled, string? userId = null)
    {
        var stats = _stats.GetOrAdd(featureName, _ => new FlagStats());

        if (enabled)
            Interlocked.Increment(ref stats.EnabledCount);
        else
            Interlocked.Increment(ref stats.DisabledCount);

        if (userId is not null)
            stats.UniqueUsers.TryAdd(userId, 0);
    }

    /// <summary>
    /// Gets the total number of evaluations for the specified feature flag.
    /// </summary>
    /// <param name="featureName">The name of the feature flag.</param>
    /// <returns>The total evaluation count, or <c>0</c> if the flag has never been evaluated.</returns>
    public long GetEvaluationCount(string featureName)
    {
        if (!_stats.TryGetValue(featureName, out var stats))
            return 0;

        return Interlocked.Read(ref stats.EnabledCount) + Interlocked.Read(ref stats.DisabledCount);
    }

    /// <summary>
    /// Gets the number of evaluations that resulted in the flag being enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag.</param>
    /// <returns>The enabled evaluation count, or <c>0</c> if the flag has never been evaluated.</returns>
    public long GetEnabledCount(string featureName)
    {
        if (!_stats.TryGetValue(featureName, out var stats))
            return 0;

        return Interlocked.Read(ref stats.EnabledCount);
    }

    /// <summary>
    /// Gets the number of evaluations that resulted in the flag being disabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag.</param>
    /// <returns>The disabled evaluation count, or <c>0</c> if the flag has never been evaluated.</returns>
    public long GetDisabledCount(string featureName)
    {
        if (!_stats.TryGetValue(featureName, out var stats))
            return 0;

        return Interlocked.Read(ref stats.DisabledCount);
    }

    /// <summary>
    /// Gets the ratio of enabled evaluations to total evaluations for the specified flag.
    /// </summary>
    /// <param name="featureName">The name of the feature flag.</param>
    /// <returns>
    /// A value between <c>0.0</c> and <c>1.0</c> representing the enabled ratio,
    /// or <c>0.0</c> if the flag has never been evaluated.
    /// </returns>
    public double GetEnabledRatio(string featureName)
    {
        if (!_stats.TryGetValue(featureName, out var stats))
            return 0.0;

        var enabled = Interlocked.Read(ref stats.EnabledCount);
        var disabled = Interlocked.Read(ref stats.DisabledCount);
        var total = enabled + disabled;

        return total == 0 ? 0.0 : (double)enabled / total;
    }

    /// <summary>
    /// Gets the number of unique users that have been evaluated for the specified flag.
    /// </summary>
    /// <param name="featureName">The name of the feature flag.</param>
    /// <returns>The unique user count, or <c>0</c> if no users have been tracked.</returns>
    public int GetUniqueUserCount(string featureName)
    {
        if (!_stats.TryGetValue(featureName, out var stats))
            return 0;

        return stats.UniqueUsers.Count;
    }

    /// <summary>
    /// Resets all recorded analytics data.
    /// </summary>
    public void Reset()
    {
        _stats.Clear();
    }

    /// <summary>
    /// Resets recorded analytics data for the specified feature flag.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to reset.</param>
    public void Reset(string featureName)
    {
        _stats.TryRemove(featureName, out _);
    }

    private sealed class FlagStats
    {
        public long EnabledCount;
        public long DisabledCount;
        public readonly ConcurrentDictionary<string, byte> UniqueUsers = new(StringComparer.Ordinal);
    }
}
