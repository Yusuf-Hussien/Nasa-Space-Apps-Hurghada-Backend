# PRD Traceability & Gap Analysis

Source of truth: `Docs/NASA_Space_Apps_Platform_PRD.docx` (v1.0, September 2026).

This document audits the implemented backend against the PRD and lists every
place the two do not line up. It is a review artifact, not a spec - the
authoritative endpoint reference is [`API_CONTRACT.md`](API_CONTRACT.md).

Legend: **Covered** = implemented to spec · **Partial** = implemented but
differs from the PRD · **Missing** = not implemented · **Extra** = implemented
but not described in the PRD.

---

## 1. Covered requirements

| PRD reference | Requirement | Implementation |
| --- | --- | --- |
| §9.1 | Team-based login, secure hashing, token sessions | `POST /auth/login/participant`, `LoginParticipantAsync`, `PasswordHasherService`, JWT |
| §6 / §10 | Authentication & authorization by role | `AppRoles` + policies `AdminOnly`/`CoreTeamOnly`/`JudgeOnly`/`StaffOnly`/`ParticipantOnly` |
| §8.2 / FR-04 | Judges search teams by id / username | `GET /teams?search=` (matches `teamId`, name, username) |
| §8.3 / §14.2 / FR-07 | Attendance per student by team; Present/Late/Absent; date + timestamp + actor | `GET /attendance/teams`, `POST /attendance`, `AttendanceStatuses`, `markedBy` |
| §9.2 / FR-06 | Requests created on mobile appear for organizers with status | `POST /requests` + `RequestsHub` group `Organizers`; `GET`/`PATCH /requests` |
| §8.2 / FR-05 | Judges submit scores against criteria; range validation; judge id + timestamp | `POST /judging/scores` (judgeId from token), upsert, `criterion.not_found`, `score.invalid`, `QuestionScore.Timestamp` (per question) |
| §8.2 | Challenge-scoped criteria with article/reference questions judges read | `JudgingCriterion.ChallengeId` + `CriterionQuestion`; `GET /judging/criteria?challengeId=` |
| §8.4 / FR-08 / §14.3 | Real-time standings board, public | `GET /judging/standings` (averaged across published judges, percentage-ranked) + `JudgingHub` `StandingsUpdated` |
| §9.4 / FR-11 / FR-12 | Sponsor tiers + Tier 1 banner config | `SponsorTier.BannerEnabled` / `Priority`, `GET /sponsors` |
| §9.3 / FR-09 / FR-10 | Questions persisted per challenge; participation tracked | `Question` (challengeId, publish, correctAnswer, points), `GameParticipation`, `answer.already_submitted` |
| §14.1 | Team login → request organizer flow | Covered end-to-end (now via team **username**, see §4) |
| §14.4 | Mobile challenge game submit answer | `POST /game/questions/{questionId}/answer` |
| §11 | Core entities exist | `Team`, `Student`, `Challenge`, `Project`, `Judge`, `JudgingCriterion`, `CriterionQuestion`, `QuestionScore`, `Attendance`, `ServiceRequest`, `Question`, `GameParticipation`, `Sponsor`, `SponsorTier` |
| §13 | Server-side validation, role authz, FK integrity | Inline validation in services; single error envelope; FK constraints |

---

## 2. Partial / divergent

| # | PRD reference | What the PRD says | What the code does | Impact |
| --- | --- | --- | --- | --- |
| D1 | §11, §18 | Scoring model has configurable `Weight` per criterion | `JudgingCriterion.Weight` is stored but **ignored**: `StandingsCalculator` ranks by **achieved percentage** (`earned/possible`), not by raw sum | **Superseded** - percentage ranking is fair across differently-sized challenges; `Weight` is vestigial |
| D2 | §9.3 | Prize-game winner rule must be explicit/configurable, "not hard-coded" | Winner ordering is **hard-coded** in `GameService.GetLeaderboardAsync` (points desc → correct-count desc → name) | Cannot configure winner rule without a code change |
| D3 | §9.1 | Team logs in with its **unique username** + password | Login used numeric `teamId` + password | **Fixed in this change** - now username + password (see §7) |
| D4 | §8.3 | Request status set: Pending, Accepted/In Progress, Resolved, Cancelled | `Pending` on create; resolve accepts `In Progress`, `Resolved`, `Rejected`, `Done`, `Cancelled` (`Done`→`Resolved`, `Cancelled`→`Rejected`). No `Accepted` | Clients must map `Accepted` → `In Progress` |
| D5 | §8.4 / §18 | Tie-breaking rules must be explicit/configurable | Ties are detected (`isTied`, competition ranking) but there is **no tie-break rule** | Tied teams share a rank |
| D6 | §13 | "database migrations" | Schema is raw `IF NOT EXISTS` SQL in `DbInitializer.SqlSchemaScript`, not EF migrations | No migration history; drift risk |
| D7 | §18 (open) | Should standings show only totals or also criterion scores? | Public board returns **overall percentage + total earned/possible + per-criterion totals**; per-question detail stays `StaffOnly` | Decision made explicitly; internal question scores never exposed publicly |
| D8 | §18 (open) | Should organizers and mentors be separate roles? | Mentor is modelled as `CoreTeam` + `memberType="Mentor"`, not a role | MemberType grants no extra permissions |

---

## 3. Missing (PRD requires, not implemented)

| # | PRD reference | Requirement | Status |
| --- | --- | --- | --- |
| M1 | §15 | `GET /teams/{teamId}/challenge` endpoint | **Closed in this change** - the public `GET /teams/{teamId}` now embeds the full challenge object (no separate route needed) |
| M2 | §8.2, §11 | Project metadata surfaced for judging (title, repo/demo links) | **Closed in this change** - public `GET /teams/{teamId}` now returns the team's `project` (title, description, repository, demo) |
| M3 | §8.1 | Public landing content: core-team members, 2024/2025 season media, event overview | **Partially addressed in this change** - `GET /seasonmedia` (public) serves season media with images; core-team member listing still `AdminOnly` (no public directory). Event overview still absent |
| M4 | §7, §9.3 | Prize / question-set grouping for the prize game | No prize entity, no grouping, no `prize` field |
| M5 | §8.3 | Request `location` field | Only `message` exists |
| M6 | §9.2 | Prevent accidental duplicate submissions | No dedupe on service requests |
| M7 | §10 | Audit trail: actor responsible for privileged actions | Only attendance records `markedBy`; request resolution stores no actor; no general audit log |
| M8 | §17 | Acceptance: sponsor display, prize persistence, etc. rely on the above | Blocked by M2-M4 |
| M9 | §8.2 | "potential questions judges can use" as a generation/analysis aid | **Largely closed in this change** - judges now read curated `CriterionQuestion` prompts per criterion via `GET /judging/criteria`. No automated *generation* of questions (still a manual authoring task) |

---

## 4. Extra (implemented, not in PRD)

| # | Feature | Where |
| --- | --- | --- |
| E1 | Staff CRUD API + `member-types` list | `StaffController` (`AdminOnly`) |
| E2 | `GET /attendance` list with filters | `AttendanceController` |
| E3 | `DELETE /requests/{id}` | `RequestsController` |
| E4 | `GET /auth/me` | `AuthController` |
| E5 | Question soft-delete + publish/unpublish workflow | `QuestionsController` |
| E6 | Game leaderboard endpoint | `GET /game/leaderboard` |
| E7 | Role-stripped responses (teams hide `username` from non-staff; questions hide `correctAnswer` from participants) | DTO `From()` mappers |
| E8 | Team detail now includes full team + challenge + roster on participant login | This change (see §7) |
| E9 | Base64 image fields + public season-media CRUD | Earlier change (see §7) |
| E10 | Article-based judging: challenge-scoped criteria, `CriterionQuestion` bank, per-question scoring, ad-hoc "other" questions, skip, **per-judge publish**, upsert, **averaged across published judges**, percentage-ranked public board | This change (see §7) |
| E11 | Public enriched team detail (challenge + project + student names, staff-only `username`) | This change (see §7) |

---

## 5. Factual divergences worth flagging

- **Ranking is by percentage, not weighted score** (D1) - the PRD's per-criterion `Weight` is stored but ignored; percentage ranking supersedes it.
- **Hard-coded prize-game winner rule** (D2) - contradicts §9.3's explicit "not hard-coded".
- **Participant login identifier** (D3) - was `teamId`, now `username` per §9.1.

---

## 6. Suggested next steps

1. Decide whether to keep `JudgingCriterion.Weight` (currently stored, ignored) or drop it in favor of percentage ranking.
2. Add a configurable prize-game winner rule (e.g. an event settings table).
3. Decide whether criterion questions need automated generation/analysis (M9 is only manually closed).
4. Add public landing content endpoints (core team directory, event overview). Season media is now served by `GET /seasonmedia` (M3, partially closed).
5. Add request dedupe and a `location` field.
6. Record the acting user on privileged mutations (requests, scores) for audit.
7. Consider EF migrations for maintainability.

---

## 7. Change log for this review

- **Participant login now uses the team username.**
  - `POST /auth/login/participant` body: `{ "username": "orion", "password": "...", "rememberMe": false }`.
  - Response now returns `teamId`, full `team` details (name, username, status), `challenge` details, and the team's `students`.
   - Closes divergence D3; enriches the login payload (E8).
- **Images as base64 (max 20 MB decoded).**
  - Stored inline in the database (`NVARCHAR(MAX)`); no upload pipeline or static hosting.
  - Fields: `Sponsor.logo`, `Staff.photo`, `Team.logo`, `Challenge.image`, `SeasonMedia.image`.
  - New routes: `PATCH /teams/{id}/logo`, `PATCH /challenges/{id}/image`, `POST`/`PUT`/`DELETE /seasonmedia`.
  - New error codes: `image.too_large`, `image.invalid_base64`, `image.invalid_type`, `season_media.not_found`, `season_media.invalid`.
   - Partially closes M3 (public season media); adds extra E9.
- **Article-based judging redesign.**
  - `JudgingCriterion` is now **challenge-scoped** (`ChallengeId`) with `IsPublished`/`PublishedAt`; new `CriterionQuestion` holds article/reference prompts (no answers, no auto-grading).
  - Judges score each question per team; new `QuestionScore` (per-question, ad-hoc "other" questions, `Skipped`, `MaxScore`, `Comment`). **Upsert** prevents duplicate rows on re-submission.
  - Each **judge publishes their own** criterion scores via `POST /judging/criteria/{id}/publish`; an **Admin** may force-publish all judges or target one judge. Only published-by-judge scores count and are **averaged across published judges**; a skip excludes only that judge's contribution.
  - **Standings rank by achieved percentage** (`earned/possible`), exposing criterion totals + `judgesCounted` only - per-question detail stays `StaffOnly`. Supersedes D1; resolves D7.
  - New routes: `GET/POST/PUT/DELETE /judging/criteria`, `POST /judging/criteria/{id}/publish|unpublish` (JudgeOrAdmin), reshaped `POST /judging/scores`, `GET /judging/scores`.
  - New error codes: `criterion.invalid`, `criterion_question.not_found`, `judging.not_allowed`.
  - Closes M1 and M2 and largely closes M9.
- **Public enriched team detail.**
  - `GET /teams/{teamId}` is now public and returns the full challenge, the project (title/description/repository/demo), and students as **names only** (no email/contact); `username` is hidden from non-staff.
  - Adds extra E11.
  - See [`JUDGING_PROCESS.md`](JUDGING_PROCESS.md) for the non-technical guide.
