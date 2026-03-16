namespace Philiprehberger.FeatureFlag;

/// <summary>
/// Provides feature flag evaluation for toggling functionality at runtime.
/// </summary>
public interface IFeatureFlags
{
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
}
