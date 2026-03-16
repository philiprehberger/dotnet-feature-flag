namespace Philiprehberger.FeatureFlag;

/// <summary>
/// Defines a feature flag with optional percentage rollout and user/role targeting.
/// </summary>
/// <param name="Enabled">Whether the feature is globally enabled.</param>
/// <param name="Percentage">
/// Optional percentage (0–100) for gradual rollout. When set, only the specified
/// percentage of users will see the feature, determined by a hash of the user ID.
/// </param>
/// <param name="AllowedUsers">
/// Optional set of user IDs that should always have the feature enabled,
/// regardless of the percentage setting.
/// </param>
/// <param name="AllowedRoles">
/// Optional set of role names that should always have the feature enabled,
/// regardless of the percentage setting.
/// </param>
public record FeatureFlagDefinition(
    bool Enabled,
    int? Percentage = null,
    HashSet<string>? AllowedUsers = null,
    HashSet<string>? AllowedRoles = null);
