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
/// <param name="EnableFrom">
/// Optional start time for the feature flag. The flag is inactive before this time,
/// even if <paramref name="Enabled"/> is <c>true</c>.
/// </param>
/// <param name="EnableUntil">
/// Optional end time for the feature flag. The flag is inactive after this time,
/// even if <paramref name="Enabled"/> is <c>true</c>.
/// </param>
/// <param name="DependsOn">
/// Optional name of another feature flag that must also be enabled for this flag
/// to be active. Creates a dependency chain between flags.
/// </param>
public record FeatureFlagDefinition(
    bool Enabled,
    int? Percentage = null,
    HashSet<string>? AllowedUsers = null,
    HashSet<string>? AllowedRoles = null,
    DateTimeOffset? EnableFrom = null,
    DateTimeOffset? EnableUntil = null,
    string? DependsOn = null);
