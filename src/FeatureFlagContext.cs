namespace Philiprehberger.FeatureFlag;

/// <summary>
/// Provides a rich evaluation context for feature flag checks, combining user identity,
/// roles, and arbitrary properties into a single object.
/// </summary>
public class FeatureFlagContext
{
    /// <summary>
    /// Gets the user identifier used for percentage rollout and user targeting.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the role names assigned to the user for role-based targeting.
    /// </summary>
    public string[]? Roles { get; init; }

    /// <summary>
    /// Gets additional custom properties that can be used for evaluation.
    /// </summary>
    public Dictionary<string, string>? Properties { get; init; }
}
