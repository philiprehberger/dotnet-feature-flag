# Changelog

## 0.2.2 (2026-03-22)

- Add dates to changelog entries

## 0.2.1 (2026-03-17)

- Rename Install section to Installation in README per package guide

## 0.2.0 (2026-03-16)

- Add `GetVariant` for A/B testing with consistent user hashing
- Add role-based access control to `IsEnabled`
- Add `FeatureFlagContext` for rich context-based evaluation

## 0.1.3 (2026-03-16)

- Add Development section to README
- Add GenerateDocumentationFile, RepositoryType, PackageReadmeFile to .csproj

## 0.1.0 (2026-03-16)

- Initial release
- Boolean feature flags with simple on/off evaluation
- Percentage-based rollout using deterministic user hashing
- User targeting with allowed-users lists
- Role-based targeting with allowed-roles lists
- Static `ForTesting` factory for unit test scenarios
- Dependency injection extensions for `IServiceCollection`
- Configuration binding from `IConfiguration` sections
