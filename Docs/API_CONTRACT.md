# NASA Space Apps Hurghada - API Contract

Contract version: `v1` · Base path: `/api/v1` · Last updated: 2026

This is the contract the mobile and web frontends integrate against. It is
generated from the actual controllers, DTO records, and service validation in
this repository - if a field or error code is listed here, it exists in code.

- Swagger UI (dev): `/swagger`
- OpenAPI JSON (dev): `/swagger/v1/swagger.json`

---

## 1. Conventions

| Topic | Rule |
| --- | --- |
| Base URL | `https://<host>/api/v1` (paths below are shown relative, e.g. `/auth/login/staff`) |
| Case | Paths are case-insensitive. JSON field names are **camelCase** on the wire. |
| Content type | `application/json` for all bodies. |
| Dates | ISO 8601, UTC (e.g. `2026-10-04T09:30:00Z`). `eventDate` query values accept `YYYY-MM-DD`. |
| Auth header | `Authorization: Bearer <accessToken>` |
| Success | `200 OK` with the resource body. Deletes return `200` with a `DeletedResponse` (not `204`). |
| Errors | Always `{ "error": { "code": "...", "message": "..." } }` (see §3). |
| 401 / 403 | A missing/invalid token returns `401`, and a role/policy failure returns `403` with an **empty body** (no error envelope). The two in-code 403s (§9.2 and §12.3) are the exception and do carry the envelope. |

All responses use terse, role-appropriate shapes: sensitive fields are stripped
per caller (e.g. teams hide `username` from non-staff; questions hide
`correctAnswer` from participants). Each endpoint below is labelled with the
policy that guards it.

---

## 2. Authentication & roles

### 2.1 Roles
`Admin`, `CoreTeam`, `Judge`, `Participant`.

Staff = `Admin | CoreTeam | Judge`. Participant = teams.

### 2.2 Authorization policies
Defined in `Program.cs`; applied per controller/action:

| Policy | Grants |
| --- | --- |
| `AdminOnly` | role `Admin` |
| `CoreTeamOnly` | role `CoreTeam` |
| `JudgeOnly` | role `Judge` |
| `StaffOnly` | `Admin`, `CoreTeam`, **or** `Judge` |
| `ParticipantOnly` | role `Participant` |

An endpoint with no policy but the `[Authorize]` attribute simply requires a
valid token (any role). Endpoints marked *Public* below are `[AllowAnonymous]`.

### 2.3 Token lifetime (`rememberMe`)
There is no refresh token, no server-side session, and no revocation list.
`rememberMe` only changes the issued JWT's expiry:

| `rememberMe` | Lifetime | Config key |
| --- | --- | --- |
| `false` | 12 hours (720 min) | `Jwt:ExpiryMinutes` |
| `true` | 30 days (43200 min) | `Jwt:RememberMeExpiryMinutes` |

Consequences clients must handle: an expired token gets a `401` - log the user
out and re-login. Deactivating a staff account does **not** invalidate tokens
already issued to them; they stay valid until expiry.

---

## 3. Error contract

Every application error is serialized as:

```json
{ "error": { "code": "team.not_found", "message": "The team does not exist." } }
```

Full catalogue of codes (HTTP status in parentheses):

| Code | Meaning |
| --- | --- |
| `auth.invalid_credentials` (401) | Bad identifier/password, or inactive account/team. |
| `user.not_found` (404) | `/auth/me` - the token's account no longer exists. |
| `user.unauthenticated` (401) | `/auth/me` - no resolvable identity. |
| `challenge.not_found` (404) | Challenge id does not exist. |
| `team.not_found` (404) | Team id does not exist. |
| `question.not_found` (404) | Question missing, inactive, or (for participants) unpublished. |
| `question.invalid` (400) | Question text/type missing or points ≤ 0. |
| `sponsor.not_found` (404) | Sponsor id does not exist. |
| `sponsor.invalid` (400) | Sponsor name missing. |
| `sponsor.tier_not_found` (400) | Referenced tier id does not exist. |
| `sponsor_tier.not_found` (404) | Tier id does not exist. |
| `sponsor_tier.invalid` (400) | Tier name missing. |
| `staff.not_found` (404) | Staff id does not exist. |
| `staff.invalid` (400) | FullName/Username/Email missing. |
| `staff.invalid_role` (400) | Role missing or not one of Admin/CoreTeam/Judge. |
| `staff.weak_password` (400) | Password shorter than 6 characters. |
| `staff.username_taken` (409) | Username already in use. |
| `staff.email_taken` (409) | Email already registered. |
| `staff.member_type_required` (400) | CoreTeam created without `memberType`. |
| `staff.invalid_member_type` (400) | `memberType` not a valid core-team type. |
| `request.invalid` (400) | Request `type`/`message` missing. |
| `request.invalid_status` (400) | Resolve status not an allowed value (§4.6). |
| `request.not_found` (404) | Request id does not exist. |
| `request.forbidden` (403) | Team-scoped delete of another team's request, or participant create for a foreign team. |
| `attendance.empty` (400) | Bulk mark with no entries. |
| `attendance.invalid_status` (400) | Status not Present/Absent/Late. |
| `attendance.students_not_found` (404) | One or more `studentId`s do not exist. |
| `game.not_participant` (403) | Answer submitted without a team identity. |
| `answer.empty` (400) | Blank answer. |
| `answer.already_submitted` (409) | Team already answered that question. |
| `judging.no_scores` (400) | `scores` list empty. |
| `criterion.not_found` (404) | Score references an unknown criterion. |
| `score.invalid` (400) | Score out of `[0, criterion.maxScore]`. |
| `judging.no_judge_link` (403) | Judge token has no linked judge profile. |

---

## 4. Enumerations & allowed values

### 4.1 Roles
`Admin`, `CoreTeam`, `Judge`, `Participant`.

### 4.2 Team status / Staff status
`"Active"` | `"Inactive"`. Only `Active` accounts/teams can log in.

### 4.3 Core-team `memberType`
`Organizer`, `Mentor`, `Developer`, `Marketing`, `PR`, `Media`, `Highboard`.
Required when `role == "CoreTeam"`; ignored (stored as `null`) for other roles.
Member type is descriptive only - it grants no extra permissions.
`GET /staff/member-types` returns this list for dropdowns.

### 4.4 Attendance status (`AttendanceStatuses`)
`Present`, `Absent`, `Late`. Case-insensitive in, canonical case out.

### 4.5 Service-request `type`
Free-form string (no server-side whitelist). Suggested UI values:
`Mentor`, `Technical`, `Logistics`, `Hardware`, `Other`.

### 4.6 Request status values
- New requests are created as `"Pending"`.
- `PATCH /requests/{id}` accepts: `In Progress`, `Resolved`, `Rejected`, `Done`, `Cancelled` (case-insensitive).
  `Done` → stored as `Resolved`; `Cancelled` → stored as `Rejected`. Other values echo back title-cased first token.
  `resolvedAt` is set when status becomes `Resolved` or `Rejected`.

---

## Endpoint quick index

| Area | Public reads | Any authenticated | Admin | CoreTeam | Judge | Participant |
| --- | --- | --- | --- | --- | --- | --- |
| Auth | login routes | `me` | - | - | - | - |
| Challenges | list, get | - | - | - | - | - |
| Teams | list | - | - | detail, students (Staff) | - | - |
| Attendance | - | list (Staff) | - | teams, bulk mark | - | - |
| Requests | - | list/create/delete | - | resolve (Staff) | - | - |
| Sponsors | list, tiers, get | - | create/update/delete | - | - | - |
| Questions | - | list, get | create/update/publish/delete | - | - | - |
| Judging | standings | criteria | - | - | submit scores | - |
| Game | leaderboard | - | - | - | - | submit answer |
| Staff | - | - | all | - | - | - |

---

## 5. Auth

### 5.1 `POST /auth/login/staff` - Public
Body:
```json
{ "identifier": "admin", "password": "Admin123!", "rememberMe": false }
```
`identifier` = username **or** email (case-insensitive). Response `200`:
```json
{
  "accessToken": "eyJ...",
  "role": "Admin",
  "name": "Event Admin",
  "username": "admin",
  "email": "admin@nasa.local",
  "staffId": 1,
  "judgeId": null
}
```
`judgeId` is non-null for Judge accounts. Errors: `auth.invalid_credentials`.

### 5.2 `POST /auth/login/participant` - Public
Body:
```json
{ "teamId": 1, "password": "Password123!", "rememberMe": false }
```
Response `200`:
```json
{
  "accessToken": "eyJ...",
  "role": "Participant",
  "name": "Orion",
  "teamId": 1,
  "username": "orion"
}
```
No `email`/`staffId`/`judgeId` fields. Errors: `auth.invalid_credentials`.

### 5.3 `GET /auth/me` - Any authenticated
Returns the same shape as the matching login route (staff shape for staff,
participant shape for a team). The token is **not** re-issued - `accessToken`
is always the empty string `""` here (the field exists only for shape
compatibility). Errors: `user.not_found`, `user.unauthenticated`.

---

## 6. Challenges - Public

### 6.1 `GET /challenges`
`200` → array of:
```json
{ "challengeId": 1, "title": "Mars Habitat Challenge", "description": "...", "source": "NASA Space Apps", "season": "2026" }
```

### 6.2 `GET /challenges/{challengeId}`
`200` → one `ChallengeResponse`. Errors: `challenge.not_found`.

---

## 7. Teams

### 7.1 `GET /teams?search=` - Public
`200` → array of `TeamSummaryResponse`. `username` is populated **only for
staff callers**; non-staff receive `"username": null`.
```json
{ "teamId": 1, "name": "Orion", "username": "orion", "status": "Active" }
```

### 7.2 `GET /teams/{teamId}` - `StaffOnly`
`200` → `TeamDetailResponse` (includes `username`):
```json
{ "teamId": 1, "name": "Orion", "username": "orion", "status": "Active", "challengeId": 1 }
```
Errors: `team.not_found`.

### 7.3 `GET /teams/{teamId}/students` - `StaffOnly`
`200` → array of:
```json
{ "studentId": 1, "name": "Aisha Ali", "email": "aisha@example.com", "contact": "555-1001", "status": "Active" }
```

---

## 8. Attendance

`AttendanceController` is class-level `StaffOnly`.

### 8.1 `GET /attendance?teamId=&eventDate=&status=` - `StaffOnly`
Filters are optional. `200` → array of:
```json
{
  "attendanceId": 1, "studentId": 1, "studentName": "Aisha Ali",
  "teamId": 1, "eventDate": "2026-10-04T00:00:00Z",
  "status": "Present", "markedBy": "coreteam", "timestamp": "2026-10-04T09:30:00Z"
}
```

### 8.2 `GET /attendance/teams?search=&eventDate=` - `CoreTeamOnly`
Check-in screen: every team with its students and their attendance for
`eventDate` (null when no date passed or not yet marked). `search` matches
team id (numeric), name, or username. `200` → array of:
```json
{
  "teamId": 1, "name": "Orion", "username": "orion", "status": "Active",
  "students": [
    { "studentId": 1, "name": "Aisha Ali", "email": "aisha@example.com",
      "contact": "555-1001", "attendanceStatus": "Present", "markedBy": "coreteam" }
  ]
}
```

### 8.3 `POST /attendance` - `CoreTeamOnly`
Bulk upsert attendance for a date (creates or overwrites per student+date).
`markedBy` is taken from the token. Body:
```json
{
  "eventDate": "2026-10-04",
  "entries": [
    { "studentId": 1, "status": "Present" },
    { "studentId": 2, "status": "Late" }
  ]
}
```
`status` ∈ `Present | Absent | Late` (case-insensitive). Response `200`:
```json
{ "updated": 1, "created": 1 }
```
Errors: `attendance.empty`, `attendance.invalid_status`, `attendance.students_not_found`.

---

## 9. Requests (service/support tickets)

`RequestsController` is class-level `[Authorize]`. Participants are always
scoped to their own team from the token (any `teamId` they send is ignored on
create, and forced on list).

### 9.1 `GET /requests?teamId=&type=&status=` - Any authenticated
Participants: `teamId` is overridden with their token's team. Staff: filters
as passed. `200` → array of:
```json
{
  "requestId": 1, "teamId": 1, "type": "Mentor", "status": "Pending",
  "message": "Need help with...", "createdAt": "2026-10-04T09:30:00Z", "resolvedAt": null
}
```

### 9.2 `POST /requests` - Any authenticated
Body:
```json
{ "teamId": 1, "type": "Mentor", "message": "Need help with..." }
```
Participants: `teamId` comes from the token. Staff: may pass any `teamId`.
Response `200` → `ServiceRequestResponse` (status starts `Pending`).
Broadcasts to the Organizers SignalR group. Errors: `team.not_found`,
`request.invalid`; in-code `403 request.forbidden` if a non-staff caller has
no team in their token.

### 9.3 `PATCH /requests/{requestId}` - `StaffOnly`
Body:
```json
{ "status": "Resolved" }
```
Response `200` → updated `ServiceRequestResponse`. Broadcasts. Errors:
`request.not_found`, `request.invalid_status`.

### 9.4 `DELETE /requests/{requestId}` - Any authenticated
Staff may delete any request; participants only their own team's. Response
`200`:
```json
{ "id": 1, "deleted": true }
```
Broadcasts. Errors: `request.not_found`, in-code `403 request.forbidden`.

---

## 10. Sponsors & tiers

### 10.1 `GET /sponsors?includeInactive=false` - Public
`200` → array of `SponsorResponse` (each embeds its `tier`):
```json
{
  "sponsorId": 1, "name": "NASA Partner", "logo": "https://...", "link": "https://...",
  "tierId": 1, "tier": { "tierId": 1, "name": "Tier 1", "priority": 1, "bannerEnabled": true },
  "active": true, "displayOrder": 1
}
```

### 10.2 `GET /sponsors/{sponsorId}` - Public
`200` → one `SponsorResponse`. Errors: `sponsor.not_found`.

### 10.3 `POST /sponsors` - `AdminOnly`
Body:
```json
{ "name": "NASA Partner", "logo": "https://...", "link": "https://...", "tierId": 1, "active": true, "displayOrder": 1 }
```
`logo`, `link`, `displayOrder` optional/nullable. The returned sponsor's `tier`
is `null` (created/updated responses omit the nested tier). Errors:
`sponsor.invalid`, `sponsor.tier_not_found`.

### 10.4 `PUT /sponsors/{sponsorId}` - `AdminOnly`
Same body & response as create. Errors: `sponsor.not_found`, `sponsor.invalid`, `sponsor.tier_not_found`.

### 10.5 `DELETE /sponsors/{sponsorId}` - `AdminOnly`
`200` → `{ "id": 1, "deleted": true }`. Errors: `sponsor.not_found`.

### 10.6 `GET /sponsors/tiers` - Public
`200` → array of:
```json
{ "tierId": 1, "name": "Tier 1", "priority": 1, "bannerEnabled": true }
```

### 10.7 `GET /sponsors/tiers/{tierId}` - Public
`200` → one tier. Errors: `sponsor_tier.not_found`.

### 10.8 `POST /sponsors/tiers` - `AdminOnly`
Body:
```json
{ "name": "Tier 1", "priority": 1, "bannerEnabled": true }
```
`priority`, `bannerEnabled` optional. `200` → tier. Errors: `sponsor_tier.invalid`.

### 10.9 `PUT /sponsors/tiers/{tierId}` - `AdminOnly`
Same body. Errors: `sponsor_tier.not_found`, `sponsor_tier.invalid`.

### 10.10 `DELETE /sponsors/tiers/{tierId}` - `AdminOnly`
`200` → `{ "id": 1, "deleted": true }`. Errors: `sponsor_tier.not_found`.

---

## 11. Questions (quiz bank)

`QuestionsController` is class-level `[Authorize]`, so even reads need a token.
Participants see only **published, active** questions and **never** the
`correctAnswer`; staff see everything including answers.

### 11.1 `GET /questions?challengeId=` - Any authenticated
`200` → array of:
```json
{
  "questionId": 1, "challengeId": 1, "type": "multiple-choice",
  "text": "What does NASA stand for?",
  "options": "[\"National Aeronautics...\", \"North American...\", \"...\"]",
  "correctAnswer": "National Aeronautics and Space Administration",
  "isPublished": true, "publishedAt": "2026-10-04T09:30:00Z", "points": 10, "isActive": true
}
```
> **`options` is a JSON-encoded string**, not a JSON array - the client must
> `JSON.parse(options)` itself. It is `null` for free-text questions.
> `correctAnswer` is `null` for participants (and for any question without one).

### 11.2 `GET /questions/{id}` - Any authenticated
`200` → one `QuestionResponse` (same visibility rules). Errors: `question.not_found`.

### 11.3 `POST /questions` - `AdminOnly`
```json
{
  "challengeId": 1, "type": "multiple-choice", "text": "Which planet...",
  "options": "[\"Venus\",\"Mars\",\"Jupiter\",\"Saturn\"]", "correctAnswer": "Mars", "points": 10
}
```
Rules: `text` required; `type` required; `points` > 0; `challengeId` must exist.
New questions start `isPublished:false`, `isActive:true`. The response includes
`correctAnswer`. Errors: `question.invalid`, `challenge.not_found`.

### 11.4 `PUT /questions/{id}` - `AdminOnly`
Same body; `challengeId` is re-pointed. Errors: `question.not_found`,
`question.invalid`.

### 11.5 `POST /questions/{id}/publish` - `AdminOnly`
Sets `isPublished:true`, stamps `publishedAt`. `200` → question. Errors: `question.not_found`.

### 11.6 `POST /questions/{id}/unpublish` - `AdminOnly`
Sets `isPublished:false`, clears `publishedAt`. Errors: `question.not_found`.

### 11.7 `DELETE /questions/{id}` - `AdminOnly`
Soft delete: sets `isActive:false`, `isPublished:false`. `200` →
`{ "id": 1, "deleted": true }`. Errors: `question.not_found`.

---

## 12. Judging

`JudgingController` is class-level `[Authorize]`.

### 12.1 `GET /judging/criteria` - Any authenticated
`200` → array of:
```json
{ "criterionId": 1, "name": "Innovation", "description": "Novelty and creativity.", "maxScore": 10, "weight": 30 }
```
> Note: requires a token (not public), unlike `/judging/standings`.

### 12.2 `GET /judging/standings` - Public
The single source of truth for the scoreboard (ranked, tie-aware). `200` →
array of:
```json
{
  "rank": 1, "isTied": false, "teamId": 1, "teamName": "Orion", "totalScore": 18.0,
  "scores": [
    { "criterionId": 1, "criterionName": "Innovation", "score": 9.5, "judgeId": 1 },
    { "criterionId": 2, "criterionName": "Technical Execution", "score": 8.5, "judgeId": 1 }
  ]
}
```
`rank` is `null` when the team has no scores yet.

### 12.3 `POST /judging/scores` - `JudgeOnly`
`judgeId` is taken from the token - **never** send it in the body. Body:
```json
{
  "teamId": 1,
  "scores": [
    { "criterionId": 1, "score": 9.5, "comment": "Strong concept" },
    { "criterionId": 2, "score": 8.5, "comment": null }
  ]
}
```
Rules: team must exist; each `criterionId` must exist; `0 ≤ score ≤ criterion.maxScore`.
`200` → array of `JudgeScoreResponse` with `judgeId` omitted (`null`):
```json
{ "scoreId": 1, "teamId": 1, "judgeId": null, "criterionId": 1,
  "criterionName": "Innovation", "score": 9.5, "comment": "Strong concept",
  "timestamp": "2026-10-04T09:30:00Z" }
```
Side effects: broadcasts `ScoresSubmitted` then `StandingsUpdated` on the
Judging hub. Errors: in-code `403 judging.no_judge_link` (account not linked to a
judge), `team.not_found`, `judging.no_scores`, `criterion.not_found`, `score.invalid`.

### 12.4 `GET /judging/scores?teamId=` - `StaffOnly`
`200` → array of `JudgeScoreResponse` (includes the real `judgeId` and
`criterionName`). `teamId` is required.

---

## 13. Game (side trophy)

### 13.1 `GET /game/leaderboard` - Public
`200` → array of:
```json
{ "rank": 1, "teamId": 1, "teamName": "Orion", "correctCount": 4, "totalPoints": 40, "isTied": false }
```
Ordering: points desc, then correct-count desc, then name.

### 13.2 `POST /game/questions/{questionId}/answer` - `ParticipantOnly`
The answering team comes from the token. One answer per team per question.
Body:
```json
{ "answer": "National Aeronautics and Space Administration" }
```
`200`:
```json
{ "correct": true, "points": 10, "submittedAt": "2026-10-04T09:30:00Z" }
```
Question must be active **and** published. Grading: free-text is
case-insensitive trimmed compare; multiple-choice accepts the option text, or
a zero-based index as a string. Errors: `question.not_found`, `answer.empty`,
`answer.already_submitted`, `game.not_participant`.

---

## 14. Staff - `AdminOnly` (whole controller)

### 14.1 `GET /staff?role=&memberType=&search=` - `AdminOnly`
`200` → array of `StaffResponse` (never exposes `passwordHash`):
```json
{
  "staffId": 1, "fullName": "Event Admin", "username": "admin", "email": "admin@nasa.local",
  "role": "Admin", "memberType": null, "status": "Active", "judgeId": null,
  "createdAt": "2026-10-04T09:30:00Z"
}
```

### 14.2 `GET /staff/{staffId}` - `AdminOnly`
`200` → one `StaffResponse`. Errors: `staff.not_found`.

### 14.3 `POST /staff` - `AdminOnly`
```json
{
  "fullName": "Sara Hassan", "username": "sara.mentor", "email": "sara.mentor@nasa.local",
  "password": "Mentor123!", "role": "CoreTeam", "memberType": "Mentor"
}
```
Rules: `fullName`/`username`/`email` required; `password` required, ≥ 6 chars;
`role` ∈ Admin/CoreTeam/Judge; CoreTeam requires a valid `memberType`. Creating
a Judge auto-creates a linked Judge profile and returns a non-null `judgeId`.
Errors: `staff.invalid`, `staff.invalid_role`, `staff.weak_password`,
`staff.username_taken`, `staff.email_taken`, `staff.member_type_required`,
`staff.invalid_member_type`.

### 14.4 `PUT /staff/{staffId}` - `AdminOnly`
```json
{
  "fullName": "Sara Hassan", "username": "sara.mentor", "email": "sara@nasa.local",
  "password": null, "role": "CoreTeam", "memberType": "Mentor", "status": "Active"
}
```
`password` is optional (omit/`null` to keep it; ≥ 6 chars if provided).
`status` is optional; only `Active`/`Inactive` are accepted (other values are
ignored). Changing the role to `Judge` auto-creates a linked judge; changing
away from `Judge` clears the link. Errors: `staff.not_found`, plus the create
errors.

### 14.4b Status shortcuts
- `PATCH /staff/{staffId}/deactivate` - sets `Inactive` (no login thereafter).
- `PATCH /staff/{staffId}/activate` - sets `Active`.
Both return the updated `StaffResponse`. Errors: `staff.not_found`.

### 14.5 `DELETE /staff/{staffId}` - `AdminOnly`
Hard delete. `200` → `{ "id": 1, "deleted": true }`. Errors: `staff.not_found`.

### 14.6 `GET /staff/member-types` - `AdminOnly`
`200` → `["Organizer","Mentor","Developer","Marketing","PR","Media","Highboard"]`.

---

## 15. Real-time (SignalR)

Hub endpoints accept the same `Authorization: Bearer <token>` (SignalR sends it
in the negotiate query for the WebSocket/SSE transport; use the standard
`accessTokenFactory`).

| Hub | URL | Access |
| --- | --- | --- |
| Requests | `/hubs/requests` | Authenticated. Staff join group `Organizers` to receive events. |
| Judging | `/hubs/judging` | Public (anonymous allowed). |

Events (server → client):

| Hub | Event | Payload |
| --- | --- | --- |
| Requests | `RequestCreated` | a `ServiceRequestResponse` |
| Requests | `RequestUpdated` | a `ServiceRequestResponse` |
| Requests | `RequestDeleted` | the deleted `requestId` (number) |
| Judging | `ScoresSubmitted` | `{ teamId, judgeId, scores: JudgeScoreResponse[] }` |
| Judging | `StandingsUpdated` | full standings array (`StandingsEntryResponse[]`) |

`RequestsHub` client calls:
- `JoinOrganizers()` - staff only; adds the caller to the `Organizers` group.
- `LeaveOrganizers()`.

---

## 16. Seed accounts (local / dev only)

Staff (login via `/auth/login/staff`): `admin`/`Admin123!`, `coreteam`/`Core123!`,
`sara.mentor`/`Mentor123!`, `omar.dev`/`Dev123!`, `judge`/`Judge123!`.

Teams (login via `/auth/login/participant`): `teamId 1` (Orion) and `teamId 2`
(Atlas), password `Password123!`.

---

## 17. Known gaps / caveats

- `GET /judging/criteria` requires a token while `GET /judging/standings` is
  public - inconsistent visibility for the scoreboard screen.
- `Judge` currently shares the full `StaffOnly` read surface (teams, attendance,
  requests resolve, scores list).
- `memberType` is informational only and grants no extra privileges.
- Deactivating a staff account does not revoke tokens already issued.
- Admin cannot check teams in for attendance (`POST /attendance` is
  `CoreTeamOnly`).
