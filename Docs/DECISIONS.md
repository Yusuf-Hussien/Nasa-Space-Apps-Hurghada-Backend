# Decisions

- Local development uses the EF Core in-memory provider when no SQL connection string is configured, while the production design remains SQL Server-first.
- JWT settings are kept in appsettings with environment-variable overrides to avoid committing secrets.
- Team authentication is implemented using the shared team credentials model described in the PRD and design contract.
