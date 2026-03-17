using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Philiprehberger.FeatureFlag;

/// <summary>
/// Evaluates feature flags with support for percentage-based rollout and user targeting.
/// </summary>
public sealed class FeatureFlags : IFeatureFlags
{
    private readonly FeatureFlagOptions _options;

    /// <summary>
    /// Creates a new <see cref="FeatureFlags"/> instance with the specified options.
    /// </summary>
    /// <param name="options">The feature flag options containing flag definitions.</param>
    public FeatureFlags(IOptions<FeatureFlagOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Creates a new <see cref="FeatureFlags"/> instance directly from options (without DI).
    /// </summary>
    /// <param name="options">The feature flag options containing flag definitions.</param>
    public FeatureFlags(FeatureFlagOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public bool IsEnabled(string featureName)
    {
        if (!_options.Flags.TryGetValue(featureName, out var definition))
            return false;

        return definition.Enabled;
    }

    /// <inheritdoc />
    public bool IsEnabled(string featureName, string userId)
    {
        return IsEnabled(featureName, userId, roles: null);
    }

    /// <inheritdoc />
    public bool IsEnabled(string featureName, string userId, string[]? roles)
    {
        if (!_options.Flags.TryGetValue(featureName, out var definition))
            return false;

        if (!definition.Enabled)
            return false;

        // Check if user is explicitly allowed
        if (definition.AllowedUsers is not null &&
            definition.AllowedUsers.Contains(userId))
            return true;

        // If percentage is set, evaluate rollout
        if (definition.Percentage is not null)
        {
            var hash = ComputePercentage(featureName, userId);
            if (hash >= definition.Percentage.Value)
                return false;
        }

        // Check role-based access control
        if (definition.AllowedRoles is { Count: > 0 })
        {
            if (roles is null || roles.Length == 0)
                return false;

            return roles.Any(r => definition.AllowedRoles.Contains(r));
        }

        return true;
    }

    /// <inheritdoc />
    public bool IsEnabled(string featureName, FeatureFlagContext context)
    {
        if (context.UserId is not null)
            return IsEnabled(featureName, context.UserId, context.Roles);

        return IsEnabled(featureName);
    }

    /// <inheritdoc />
    public string? GetVariant(string featureName, string userId, string[] variants)
    {
        if (variants.Length == 0)
            return null;

        var hash = ComputeHash(featureName, userId);
        var index = (int)(hash % (uint)variants.Length);
        return variants[index];
    }

    /// <summary>
    /// Creates a <see cref="FeatureFlags"/> instance for use in unit tests.
    /// </summary>
    /// <param name="flags">Tuples of feature name and enabled state.</param>
    /// <returns>A configured <see cref="FeatureFlags"/> instance.</returns>
    public static FeatureFlags ForTesting(params (string name, bool enabled)[] flags)
    {
        var definitions = new Dictionary<string, FeatureFlagDefinition>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, enabled) in flags)
        {
            definitions[name] = new FeatureFlagDefinition(enabled);
        }

        return new FeatureFlags(new FeatureFlagOptions { Flags = definitions });
    }

    /// <summary>
    /// Computes a deterministic hash value from a feature name and user ID.
    /// </summary>
    private static uint ComputeHash(string featureName, string userId)
    {
        var input = $"{featureName}:{userId}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToUInt32(hash, 0);
    }

    /// <summary>
    /// Computes a deterministic percentage (0–99) from a feature name and user ID
    /// to ensure consistent rollout assignment.
    /// </summary>
    private static int ComputePercentage(string featureName, string userId)
    {
        return (int)(ComputeHash(featureName, userId) % 100);
    }
}
