namespace Philiprehberger.FeatureFlag;

/// <summary>
/// Provides feature flag evaluation for toggling functionality at runtime.
/// </summary>
public interface IFeatureFlags
{
    /// <summary>
    /// Gets the analytics tracker for recording and querying flag evaluation statistics.
    /// </summary>
    FeatureFlagAnalytics Analytics { get; }
    /// <summary>
    /// Checks whether the specified feature is globally enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to evaluate.</param>
    /// <returns><c>true</c> if the feature is enabled; otherwise, <c>false</c>.</returns>
    bool IsEnabled(string featureName);

    /// <summary>
    /// Checks whether the specified feature is enabled for a given user.
    /// Supports percentage-based rollout and user/role targeting.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to evaluate.</param>
    /// <param name="userId">The user identifier used for percentage rollout and targeting.</param>
    /// <returns><c>true</c> if the feature is enabled for the user; otherwise, <c>false</c>.</returns>
    bool IsEnabled(string featureName, string userId);

    /// <summary>
    /// Checks whether the specified feature is enabled for a given user with optional role-based access control.
    /// When the flag defines <see cref="FeatureFlagDefinition.AllowedRoles"/>, the user's roles are checked
    /// after percentage evaluation.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to evaluate.</param>
    /// <param name="userId">The user identifier used for percentage rollout and targeting.</param>
    /// <param name="roles">Optional role names assigned to the user for role-based targeting.</param>
    /// <returns><c>true</c> if the feature is enabled for the user; otherwise, <c>false</c>.</returns>
    bool IsEnabled(string featureName, string userId, string[]? roles);

    /// <summary>
    /// Checks whether the specified feature is enabled using a rich evaluation context.
    /// Supports user targeting, percentage rollout, and role-based access control via
    /// a single <see cref="FeatureFlagContext"/> object.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to evaluate.</param>
    /// <param name="context">The evaluation context containing user ID, roles, and custom properties.</param>
    /// <returns><c>true</c> if the feature is enabled for the given context; otherwise, <c>false</c>.</returns>
    bool IsEnabled(string featureName, FeatureFlagContext context);

    /// <summary>
    /// Returns a variant name for the given user, based on consistent hashing.
    /// Useful for A/B testing where users must be deterministically assigned to a variant.
    /// </summary>
    /// <param name="featureName">The name of the feature flag to select a variant for.</param>
    /// <param name="userId">The user identifier used for deterministic variant assignment.</param>
    /// <param name="variants">The available variant names to choose from.</param>
    /// <returns>The selected variant name, or <c>null</c> if <paramref name="variants"/> is empty.</returns>
    string? GetVariant(string featureName, string userId, string[] variants);
}
