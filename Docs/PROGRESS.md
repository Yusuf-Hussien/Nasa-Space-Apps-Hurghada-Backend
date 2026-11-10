# Progress

## Phase 1 - Backend foundation
- [x] Reviewed the PRD and backend design contract.
- [x] Created the ASP.NET Core solution and project layers.
- [x] Implemented the domain model and EF Core context.
- [x] Added team authentication and JWT issuance.
- [x] Added challenge, team, request, attendance, judging, standings, and sponsor endpoints.
- [x] Run end-to-end build/test verification and fix any remaining issues.
- [x] Redesigned judging for 2026: five global criteria, criterion-level 1.0-5.0 scores, optional reference prompts.
- [x] Added the one-time `2026-criterion-judging-v1` data migration and updated judging tests.

## Known decisions
- Local execution defaults to the in-memory EF provider when no SQL Server connection string is configured.
- Development JWT secret is loaded from appsettings and can be overridden using environment variables.
- Judging is criterion-level only: one score per judge, team, and global criterion; question-level/ad-hoc/skip scoring is retired.
- `MaxScore` and `Weight` are locked at `5` and `20`; weight is display-only and does not affect standings.
