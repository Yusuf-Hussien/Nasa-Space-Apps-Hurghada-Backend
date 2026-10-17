# Progress

## Phase 1 - Backend foundation
- [x] Reviewed the PRD and backend design contract.
- [x] Created the ASP.NET Core solution and project layers.
- [x] Implemented the domain model and EF Core context.
- [x] Added team authentication and JWT issuance.
- [x] Added challenge, team, request, attendance, judging, standings, and sponsor endpoints.
- [ ] Run end-to-end build/test verification and fix any remaining issues.

## Known decisions
- Local execution defaults to the in-memory EF provider when no SQL Server connection string is configured.
- Development JWT secret is loaded from appsettings and can be overridden using environment variables.
