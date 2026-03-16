# Philiprehberger.FeatureFlag

[![CI](https://github.com/philiprehberger/dotnet-feature-flag/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-feature-flag/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.FeatureFlag.svg)](https://www.nuget.org/packages/Philiprehberger.FeatureFlag)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-feature-flag)](LICENSE)

Lightweight feature flags with percentage rollout and user targeting — no external service required.

## Install

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

### `FeatureFlagDefinition`

| Property | Type | Description |
|----------|------|-------------|
| `Enabled` | `bool` | Whether the feature is globally enabled |
| `Percentage` | `int?` | Optional percentage (0-100) for gradual rollout |
| `AllowedUsers` | `HashSet<string>?` | User IDs that always have the feature enabled |
| `AllowedRoles` | `HashSet<string>?` | Role names that always have the feature enabled |

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
```

## License

MIT
