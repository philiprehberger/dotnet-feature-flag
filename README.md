# Philiprehberger.FeatureFlag

[![CI](https://github.com/philiprehberger/dotnet-feature-flag/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-feature-flag/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.FeatureFlag.svg)](https://www.nuget.org/packages/Philiprehberger.FeatureFlag)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-feature-flag)](LICENSE)

Lightweight feature flags with percentage rollout and user targeting — no external service required.

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

### `FeatureFlagDefinition`

| Property | Type | Description |
|----------|------|-------------|
| `Enabled` | `bool` | Whether the feature is globally enabled |
| `Percentage` | `int?` | Optional percentage (0-100) for gradual rollout |
| `AllowedUsers` | `HashSet<string>?` | User IDs that always have the feature enabled |
| `AllowedRoles` | `HashSet<string>?` | Role names that always have the feature enabled |

### `FeatureFlagContext`

| Property | Type | Description |
|----------|------|-------------|
| `UserId` | `string?` | User identifier for percentage rollout and user targeting |
| `Roles` | `string[]?` | Role names for role-based targeting |
| `Properties` | `Dictionary<string, string>?` | Custom properties for evaluation |

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
