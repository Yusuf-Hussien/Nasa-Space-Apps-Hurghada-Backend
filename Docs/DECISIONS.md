# Decisions

- Local development uses the EF Core in-memory provider when no SQL connection string is configured, while the production design remains SQL Server-first.
- JWT settings are kept in appsettings with environment-variable overrides to avoid committing secrets.
- Team authentication is implemented using the shared team credentials model described in the PRD and design contract.
- Judging uses five global criteria (Impact, Creativity, Validity, Relevance, Presentation) with one 1.0-5.0 score per judge/team/criterion. Question-level, ad-hoc, and skip scoring are retired; `CriterionQuestion` remains optional reference prompts only.
- Criterion `MaxScore` and `Weight` are locked at `5` and `20`. Weight is display-only; standings rank by achieved percentage averaged across publishing judges.
- A one-time transactional startup migration (`2026-criterion-judging-v1`) resets criteria, publications, judge scores, and legacy question scores, then seeds the five global criteria. It is marked in `AppMigrations` so it runs once per database.
