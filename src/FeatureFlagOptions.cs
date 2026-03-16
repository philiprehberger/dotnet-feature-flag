namespace Philiprehberger.FeatureFlag;

/// <summary>
/// Configuration options containing all feature flag definitions.
/// </summary>
public record FeatureFlagOptions
{
    /// <summary>
    /// Gets the dictionary of feature flag definitions keyed by feature name.
    /// </summary>
    public Dictionary<string, FeatureFlagDefinition> Flags { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
