using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Philiprehberger.FeatureFlag;

/// <summary>
/// Extension methods for registering feature flag services with dependency injection.
/// </summary>
public static class FeatureFlagServiceCollectionExtensions
{
    /// <summary>
    /// Adds feature flag services to the service collection using an options configuration delegate.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">A delegate to configure <see cref="FeatureFlagOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        Action<FeatureFlagOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IFeatureFlags, FeatureFlags>();
        return services;
    }

    /// <summary>
    /// Adds feature flag services to the service collection by binding to an <see cref="IConfiguration"/> section.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The configuration section containing feature flag definitions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddFeatureFlags(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FeatureFlagOptions>(configuration);
        services.AddSingleton<IFeatureFlags, FeatureFlags>();
        return services;
    }
}
