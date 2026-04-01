# Philiprehberger.FeatureFlag

[![CI](https://github.com/philiprehberger/dotnet-feature-flag/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-feature-flag/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.FeatureFlag.svg)](https://www.nuget.org/packages/Philiprehberger.FeatureFlag)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-feature-flag)](https://github.com/philiprehberger/dotnet-feature-flag/commits/main)

Lightweight feature flags with percentage rollout, user targeting, time-based scheduling, and analytics — no external service required.

## Installation

```bash
dotnet add package Philiprehberger.FeatureFlag
```

## Usage

```csharp
using Philiprehberger.FeatureFlag;
```

### Simple Boolean Flags

```csharp
var flags = new FeatureFlags(new FeatureFlagOptions
{
    Flags = new Dictionary<string, FeatureFlagDefinition>
    {
        ["dark-mode"] = new(Enabled: true),
        ["beta-feature"] = new(Enabled: false)
    }
});

if (flags.IsEnabled("dark-mode"))
{
    // feature is on
}
```

### Percentage Rollout

Roll out a feature to a percentage of users. Assignment is deterministic based on user ID, so a given user always sees the same result.

```csharp
var flags = new FeatureFlags(new FeatureFlagOptions
{
    Flags = new Dictionary<string, FeatureFlagDefinition>
    {
        ["new-checkout"] = new(Enabled: true, Percentage: 25)
    }
});

if (flags.IsEnabled("new-checkout", userId: "user-42"))
{
    // 25% of users see this
}
```

### User Targeting

Always enable a feature for specific users, regardless of percentage:

```csharp
var flags = new FeatureFlags(new FeatureFlagOptions
{
    Flags = new Dictionary<string, FeatureFlagDefinition>
    {
        ["new-checkout"] = new(
            Enabled: true,
            Percentage: 10,
            AllowedUsers: new HashSet<string> { "admin-1", "beta-tester-5" })
    }
});

// Always true for allowed users, even though percentage is 10%
flags.IsEnabled("new-checkout", "admin-1"); // true
```

### Role-Based Access

Restrict a feature to users with specific roles. When `AllowedRoles` is defined, the user must hold at least one matching role:

```csharp
var flags = new FeatureFlags(new FeatureFlagOptions
{
    Flags = new Dictionary<string, FeatureFlagDefinition>
    {
        ["admin-dashboard"] = new(
            Enabled: true,
            AllowedRoles: new HashSet<string> { "admin", "manager" })
    }
});

flags.IsEnabled("admin-dashboard", "user-1", roles: new[] { "admin" }); // true
flags.IsEnabled("admin-dashboard", "user-2", roles: new[] { "viewer" }); // false
```

### Time-Based Scheduling

Schedule a feature to activate within a specific time window. The flag is only active between `EnableFrom` and `EnableUntil`:

```csharp
var flags = new FeatureFlags(new FeatureFlagOptions
{
    Flags = new Dictionary<string, FeatureFlagDefinition>
    {
        ["holiday-sale"] = new(
            Enabled: true,
            EnableFrom: new DateTimeOffset(2026, 12, 20, 0, 0, 0, TimeSpan.Zero),
            EnableUntil: new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero))
    }
});

// Only returns true between Dec 20 and Dec 31, 2026
flags.IsEnabled("holiday-sale");
```

Either bound is optional — use `EnableFrom` alone for a start date, or `EnableUntil` alone for an expiration.

### Flag Dependencies

Make a flag depend on another flag being enabled. The dependent flag is only active when its parent is also active:

```csharp
var flags = new FeatureFlags(new FeatureFlagOptions
{
    Flags = new Dictionary<string, FeatureFlagDefinition>
    {
        ["new-ui"] = new(Enabled: true),
        ["new-ui-sidebar"] = new(Enabled: true, DependsOn: "new-ui")
    }
});

flags.IsEnabled("new-ui-sidebar"); // true (new-ui is enabled)
```

Dependencies are resolved recursively, so chained dependencies work as expected.

### Analytics

Track evaluation statistics per flag, including counts, ratios, and unique users:

```csharp
var flags = new FeatureFlags(new FeatureFlagOptions
{
    Flags = new Dictionary<string, FeatureFlagDefinition>
    {
        ["feature"] = new(Enabled: true, Percentage: 50)
    }
});

flags.IsEnabled("feature", "user-1");
flags.IsEnabled("feature", "user-2");
flags.IsEnabled("feature", "user-3");

long total = flags.Analytics.GetEvaluationCount("feature");
long enabled = flags.Analytics.GetEnabledCount("feature");
double ratio = flags.Analytics.GetEnabledRatio("feature");
int users = flags.Analytics.GetUniqueUserCount("feature");
```

Analytics are recorded automatically on every `IsEnabled` call. Use `Analytics.Reset()` to clear all data or `Analytics.Reset("feature")` for a single flag.

### A/B Variants

Assign users to experiment variants deterministically. The same user always receives the same variant for a given feature:

```csharp
var variant = flags.GetVariant(
    "checkout-experiment",
    userId: "user-42",
    variants: new[] { "control", "variant-a", "variant-b" });

// variant is consistently one of the three options for this user
```

### Context-Based Evaluation

Use `FeatureFlagContext` to pass user identity, roles, and custom properties in a single object:

```csharp
var context = new FeatureFlagContext
{
    UserId = "user-42",
    Roles = new[] { "beta-tester" },
    Properties = new Dictionary<string, string>
    {
        ["region"] = "eu-west",
        ["plan"] = "pro"
    }
};

if (flags.IsEnabled("new-checkout", context))
{
    // feature is on for this context
}
```

### Dependency Injection

Register with `IServiceCollection` using a configuration delegate:

```csharp
builder.Services.AddFeatureFlags(options =>
{
    options.Flags["dark-mode"] = new FeatureFlagDefinition(Enabled: true);
    options.Flags["new-checkout"] = new FeatureFlagDefinition(Enabled: true, Percentage: 50);
});
```

Or bind from `IConfiguration` (e.g., `appsettings.json`):

```csharp
builder.Services.AddFeatureFlags(builder.Configuration.GetSection("FeatureFlags"));
```

### Testing

Use the static `ForTesting` factory to create flags in unit tests without DI:

```csharp
var flags = FeatureFlags.ForTesting(
    ("dark-mode", true),
    ("beta-feature", false));

Assert.True(flags.IsEnabled("dark-mode"));
Assert.False(flags.IsEnabled("beta-feature"));
```

## API

### `IFeatureFlags`

| Method | Description |
|--------|-------------|
| `IsEnabled(featureName)` | Checks if a feature is globally enabled |
| `IsEnabled(featureName, userId)` | Checks if a feature is enabled for a specific user (supports percentage rollout and targeting) |
| `IsEnabled(featureName, userId, roles)` | Checks if a feature is enabled with role-based access control |
| `IsEnabled(featureName, context)` | Checks if a feature is enabled using a `FeatureFlagContext` |
| `GetVariant(featureName, userId, variants)` | Returns a deterministic variant name for A/B testing |
| `Analytics` | Gets the `FeatureFlagAnalytics` instance for evaluation statistics |

### `FeatureFlagDefinition`

| Property | Type | Description |
|----------|------|-------------|
| `Enabled` | `bool` | Whether the feature is globally enabled |
| `Percentage` | `int?` | Optional percentage (0-100) for gradual rollout |
| `AllowedUsers` | `HashSet<string>?` | User IDs that always have the feature enabled |
| `AllowedRoles` | `HashSet<string>?` | Role names that always have the feature enabled |
| `EnableFrom` | `DateTimeOffset?` | Optional start time for the feature flag |
| `EnableUntil` | `DateTimeOffset?` | Optional end time for the feature flag |
| `DependsOn` | `string?` | Optional name of a dependency flag that must also be enabled |

### `FeatureFlagContext`

| Property | Type | Description |
|----------|------|-------------|
| `UserId` | `string?` | User identifier for percentage rollout and user targeting |
| `Roles` | `string[]?` | Role names for role-based targeting |
| `Properties` | `Dictionary<string, string>?` | Custom properties for evaluation |

### `FeatureFlagAnalytics`

| Method | Description |
|--------|-------------|
| `Record(featureName, enabled, userId?)` | Records a flag evaluation result |
| `GetEvaluationCount(featureName)` | Gets total evaluation count for a flag |
| `GetEnabledCount(featureName)` | Gets the number of enabled evaluations |
| `GetDisabledCount(featureName)` | Gets the number of disabled evaluations |
| `GetEnabledRatio(featureName)` | Gets the ratio of enabled to total evaluations (0.0-1.0) |
| `GetUniqueUserCount(featureName)` | Gets the number of unique users evaluated |
| `Reset()` | Clears all analytics data |
| `Reset(featureName)` | Clears analytics data for a specific flag |

### `FeatureFlagOptions`

| Property | Type | Description |
|----------|------|-------------|
| `Flags` | `Dictionary<string, FeatureFlagDefinition>` | Flag definitions keyed by feature name (case-insensitive) |

### `FeatureFlagServiceCollectionExtensions`

| Method | Description |
|--------|-------------|
| `AddFeatureFlags(Action<FeatureFlagOptions>)` | Registers feature flags with a configuration delegate |
| `AddFeatureFlags(IConfiguration)` | Registers feature flags from a configuration section |

## Development

```bash
dotnet build src/Philiprehberger.FeatureFlag.csproj --configuration Release
dotnet test tests/Philiprehberger.FeatureFlag.Tests/Philiprehberger.FeatureFlag.Tests.csproj --configuration Release
```

## Support

If you find this project useful:

⭐ [Star the repo](https://github.com/philiprehberger/dotnet-feature-flag)

🐛 [Report issues](https://github.com/philiprehberger/dotnet-feature-flag/issues?q=is%3Aissue+is%3Aopen+label%3Abug)

💡 [Suggest features](https://github.com/philiprehberger/dotnet-feature-flag/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)

❤️ [Sponsor development](https://github.com/sponsors/philiprehberger)

🌐 [All Open Source Projects](https://philiprehberger.com/open-source-packages)

💻 [GitHub Profile](https://github.com/philiprehberger)

🔗 [LinkedIn Profile](https://www.linkedin.com/in/philiprehberger)

## License

[MIT](LICENSE)
