# Backend Roadmap — Multi-Role System (Backend First)

> Written: 2026-10-17. Status: **IN PROGRESS**
> Frontend: `C:\Users\20127\Downloads\Nasa-Space-Apps-Hurghada-Frontend` (React/Vite, `:5173`, currently 100% static — zero API calls).
> Mobile app (participant) will consume the **same `api/v1`** surface.

---

## 1. Confirmed Requirements

| Item | Decision |
|---|---|
| Roles | `Admin`, `CoreTeam`, `Judge`, `Participant` |
| Participant auth | **`teamId` + `password`** (unified per team — web + mobile) |
| Staff auth (Admin/CoreTeam/Judge) | **`identifier`** = email *or* username + `password` (frontend will add a switch button later) |
| Attendance | CoreTeam sees **all teams**, searches by **teamId or name**, marks attendees, **can re-mark/backfill later** (upsert) |
| CORS | **Allow ALL origins** now, tighten post-deploy |
| Scope | **Backend only** for now; frontend integration next |
| Admin powers | Full access, **publish questions**, **add CoreTeam** |
| CoreTeam powers | **Take attendance** |
| Judge powers | **Judge based on given criteria** |
| Participant powers | Request organizer/mentor, **answer asked questions for side trophy** |

### Decisions taken (defaults, editable)
- **Schema strategy (A)**: EF Core **migrations** + `Database.Migrate()` (safer for any existing SQL DB). InMemory unaffected.
- **Legacy judges**: existing `Judge.Account` rows have no password → **seed-only judges going forward** (Admin can add later). Legacy `Judge` table kept, linked from `Staff.JudgeId`.

---

## 2. Current State (baseline)

**Frontend**: `/login` posts `{ identifier, password, rememberMe }` client-side only. No token storage, no route guards, no role UI.

**Backend** (ASP.NET Core 8, builds clean):
- Team-only auth; JWT `role` claim wrongly carries `team.Status` ("Active").
- No `[Authorize]` anywhere — every endpoint anonymous.
- No CORS configured.
- Questions/Game have domain models + repo methods but **zero controllers**.
- Attendance is write-only (no `GET`, no upsert/backfill, no team search).

---

## 3. API Contract (web + mobile share)

```
POST /api/v1/auth/login         { identifier?, teamId?, password } → { accessToken, role, ... }
GET  /api/v1/auth/me            [any authenticated]

GET  /api/v1/attendance/teams?search=&eventDate=   [CoreTeam]   ← search teamId or name
POST /api/v1/attendance         { eventDate, entries:[{studentId,status}] } [CoreTeam] ← upsert
GET  /api/v1/attendance?teamId=&eventDate=          [Staff]

GET  /api/v1/questions          [Participant → published only, no answers; Staff → all]
GET  /api/v1/questions/{id}
POST /api/v1/questions          [Admin]
PUT  /api/v1/questions/{id}     [Admin]
POST /api/v1/questions/{id}/publish   [Admin]
POST /api/v1/questions/{id}/unpublish [Admin]
DELETE /api/v1/questions/{id}   [Admin] (soft)

POST /api/v1/questions/{id}/answer  [Participant] → { correct, points, submittedAt }
GET  /api/v1/game/leaderboard        [anonymous — side trophy board]

POST /api/v1/judging/scores      [Judge]  judgeId from token
GET  /api/v1/judging/criteria    [authenticated]
GET  /api/v1/judging/scores?teamId=  [Judge/Admin]

POST /api/v1/requests           [Participant — teamId from token]
GET  /api/v1/requests           [Staff all / Participant own team]
PATCH /api/v1/requests/{id}     [Staff — resolve]

GET  /api/v1/teams, /challenges, /standings, /sponsors  (public/staff per table)
```

---

## 4. Phases

### Phase 1 — Domain (`DomainModels.cs`)
- `AppRoles` constants (Admin/CoreTeam/Judge/Participant).
- New **`Staff`** entity: `StaffId, FullName, Username*, Email*, PasswordHash, Role, Status, JudgeId?` — one login pool for non-participant roles. Keeps legacy `Judge` table (scores FK intact), linked via `JudgeId`.
- `Question` gains `IsPublished, PublishedAt, Points, IsActive`.

### Phase 2 — Infrastructure
- `DbSet<Staff>` + unique indexes on `Username`/`Email`; unique `(StudentId, EventDate)` on Attendance (upsert); unique `(QuestionId, TeamId)` on GameParticipation.
- Repository: `GetStaffByIdentifierAsync` (username OR email), staff CRUD, `GetAttendanceAsync(teamId/search/date)`, **`UpsertAttendanceAsync`**, question CRUD, participation + leaderboard queries.
- `ITokenService` → `CreateParticipantToken(Team)` + `CreateStaffToken(Staff)`, **role claim fixed**, `teamId`/`staffId`/`judgeId` claims, 12h expiry (`Jwt:ExpiryMinutes`).
- Implement the declared-but-unused `ICurrentUser` → services stop trusting body-supplied IDs.
- Seed: `admin/Admin123!`, `coreteam/Core123!`, `judge/Judge123!` (linked to seeded judge) + sample questions.

### Phase 3 — Application services
- **`AuthService`**: `LoginUnifiedAsync(identifier?, teamId?, password)` → participant (teamId+password) or staff (email-or-username+password); returns `role`; + `GET /auth/me`.
- **`QuestionService`** (new): create-draft, publish/unpublish, list — Admin sees all, Participant sees published **without correct answers**.
- **`GameService`** (new): submit answer (Participant, one-per-team, graded, 409 on duplicate) + leaderboard.
- **`AttendanceService`**: `GetTeamsWithAttendanceAsync(search, eventDate)` (search matches numeric id OR name), `MarkBulkAsync` upsert with `MarkedBy` from token.
- **`RequestService`**: teamId from token; add resolve for staff.
- **`JudgingService`**: `judgeId` derived from token, reject non-Judge roles.

### Phase 4 — Api layer
- **`Program.cs`**: `AddCors(AllowAll)` + `app.UseCors` (before auth); policies `AdminOnly / CoreTeamOnly / JudgeOnly / StaffOnly / ParticipantOnly`; register `ICurrentUser`.
- Controllers: Auth (unified login + `/me`), Attendance (GET teams + POST bulk, CoreTeamOnly), **new QuestionsController** (CRUD+publish, Admin), **new GameController** (answer + leaderboard), plus `[Authorize]` across Teams/Judging/Requests. Standings/Sponsors/Challenges stay public.
- FluentValidation validators for new request DTOs.

### Phase 5 — DB strategy
`EnsureCreated()` → **EF migrations + `Database.Migrate()`** (Option A). Seed only when empty.

### Phase 6 — Tests + docs
Auth (both login modes + wrong password), attendance (search/backfill upsert), question publish visibility, answer grading/duplicate, CORS preflight (`access-control-allow-origin: *`), 401/403 checks. Update `README`, `Docs/PROGRESS.md`, `Docs/DECISIONS.md`.

---

## 5. Seed accounts (dev)

| Role | Username | Email | Password | Link |
|---|---|---|---|---|
| Admin | `admin` | `admin@nasa.local` | `Admin123!` | — |
| CoreTeam | `coreteam` | `core@nasa.local` | `Core123!` | — |
| Judge | `judge` | `judge@nasa.local` | `Judge123!` | → seeded `Dr. Ada Smith` |
| Team | `orion` / `atlas` | — | `Password123!` | Participant (teamId+password) |
