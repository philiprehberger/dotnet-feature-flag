# Changelog

## 0.2.0

- Add `GetVariant` for A/B testing with consistent user hashing
- Add role-based access control to `IsEnabled`
- Add `FeatureFlagContext` for rich context-based evaluation

## 0.1.3

- Add Development section to README
- Add GenerateDocumentationFile, RepositoryType, PackageReadmeFile to .csproj

## 0.1.0 (2026-03-15)

- Initial release
- Boolean feature flags with simple on/off evaluation
- Percentage-based rollout using deterministic user hashing
- User targeting with allowed-users lists
- Role-based targeting with allowed-roles lists
- Static `ForTesting` factory for unit test scenarios
- Dependency injection extensions for `IServiceCollection`
- Configuration binding from `IConfiguration` sections
