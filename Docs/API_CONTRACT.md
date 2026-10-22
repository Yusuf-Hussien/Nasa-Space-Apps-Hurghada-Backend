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

### 1.1 Images

Images are transported as **base64 strings** and stored in the database - there
is no file upload and no static-file hosting. A single image field may hold
either:

- plain base64 (e.g. `"iVBORw0KGgo..."`), or
- a full data URI (e.g. `"data:image/png;base64,iVBORw0KGgo..."`), or
- a legacy absolute `http(s)` URL (passed through untouched).

Limits and rules (`ImageValidator`):

| Rule | Value |
| --- | --- |
| Max size | **20 MB measured after base64-decoding** (so the JSON string is ~26.7 MB) |
| Allowed types (when a data URI is used) | `image/png`, `image/jpeg`, `image/jpg`, `image/webp`, `image/gif`, `image/svg+xml` |
| Encoding | standard base64 |
| Clearing | send `null` or `""` to remove the image |

Image fields live on: `Sponsor.logo`, `Staff.photo` (core team), `Team.logo`,
`Challenge.image`, and `SeasonMedia.image`. They are written either inline on
the owning `POST`/`PUT` payload (sponsor, staff, season media) or via a
dedicated `PATCH .../image` / `.../logo` route (challenge, team).

On failure the standard error envelope is returned with one of:
`image.invalid_type`, `image.invalid_base64`, `image.too_large`.

> Because images are stored inline in the database, list endpoints that return
> them (teams, sponsors, season media, challenges) can be large. Prefer sending
> the raw base64 with a data URI prefix so the client can render directly.

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
| `auth.invalid_credentials` (401) | Bad identifier/username/password, or inactive account/team. |
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
| `judging.no_scores` (400) | `scores` list empty, or no scores exist to publish for that judge/criterion. |
| `criterion.not_found` (404) | Criterion (or scored criterion) does not exist. |
| `criterion.invalid` (400) | Criterion name missing or max score not positive. |
| `criterion_question.not_found` (404) | Scored question does not belong to the given criterion. |
| `score.invalid` (400) | Score out of `[0, maxScore]`, or max score not positive. |
| `judging.no_judge_link` (403) | Judge token has no linked judge profile. |
| `judging.not_allowed` (403) | Only a judge or an admin may publish criterion scores. |
| `season_media.not_found` (404) | Season-media id does not exist. |
| `season_media.invalid` (400) | Season or title missing. |
| `image.invalid_type` (400) | Data-URI MIME type not in the allowed image list. |
| `image.invalid_base64` (400) | Image value is not valid base64. |
| `image.too_large` (400) | Image exceeds the 20 MB limit after decoding. |

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
| Challenges | list, get | - | set image | - | - | - |
| Teams | list, detail | - | set logo | students (Staff) | - | - |
| Attendance | - | list (Staff) | - | teams, bulk mark | - | - |
| Requests | - | list/create/delete | - | resolve (Staff) | - | - |
| Sponsors | list, tiers, get | - | create/update/delete | - | - | - |
| Questions | - | list, get | create/update/publish/delete | - | - | - |
| Judging | criteria, standings | - | criteria CRUD | scores (Staff) | submit scores, publish/unpublish own scores | - |
| Game | leaderboard | - | - | - | - | submit answer |
| Season media | list, get | - | create/update/delete | - | - | - |
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
`username` is the team's unique login username (case-insensitive). Body:
```json
{ "username": "orion", "password": "Password123!", "rememberMe": false }
```
Response `200` - carries the team's own record, its challenge, and its roster
so the mobile dashboard needs no extra calls:
```json
{
  "accessToken": "eyJ...",
  "role": "Participant",
  "teamId": 1,
  "team": { "teamId": 1, "name": "Orion", "username": "orion", "status": "Active" },
  "challenge": {
    "challengeId": 1, "title": "Mars Habitat Challenge",
    "description": "...", "source": "NASA Space Apps", "season": "2026"
  },
  "students": [
    { "studentId": 1, "name": "Aisha Ali", "email": "aisha@example.com", "contact": "555-1001", "status": "Active" }
  ]
}
```
`challenge.challengeId` is `0` and the text fields are `null` if the team has no
linked challenge. No `email`/`staffId`/`judgeId` fields at the top level.
Errors: `auth.invalid_credentials`.

### 5.3 `GET /auth/me` - Any authenticated
Returns the same shape as the matching login route (staff shape for staff, the
participant shape of §5.2 for a team - including `team`, `challenge`, and
`students`). The token is **not** re-issued - `accessToken` is always the empty
string `""` here (the field exists only for shape compatibility).
Errors: `user.not_found`, `user.unauthenticated`.

---

## 6. Challenges - Public

### 6.1 `GET /challenges`
`200` → array of:
```json
{ "challengeId": 1, "title": "Mars Habitat Challenge", "description": "...", "source": "NASA Space Apps", "season": "2026", "image": "data:image/png;base64,..." }
```
`image` is null when unset (§1.1).

### 6.2 `GET /challenges/{challengeId}`
`200` → one `ChallengeResponse`. Errors: `challenge.not_found`.

### 6.3 `PATCH /challenges/{challengeId}/image` - `AdminOnly`
Body: `{ "image": "<base64|data-uri|url|null>" }`. `200` → updated
`ChallengeResponse`. Errors: `challenge.not_found`, `image.invalid_type`,
`image.invalid_base64`, `image.too_large`.

---

## 7. Teams

### 7.1 `GET /teams?search=` - Public
`200` → array of `TeamSummaryResponse`. `username` is populated **only for
staff callers**; non-staff receive `"username": null`. `logo` is public.
```json
{ "teamId": 1, "name": "Orion", "username": "orion", "logo": null, "status": "Active" }
```

### 7.2 `GET /teams/{teamId}` - Public
`200` → `TeamDetailResponse`: the team plus its full `challenge`, its `project`
(title/description/repository/demo links), and its `students` as **names only**
(no email/contact). `username` is populated **only for staff callers** (public →
`null`). Member management (`GET /teams/{teamId}/students`) stays `StaffOnly`.
```json
{
  "teamId": 1, "name": "Orion", "username": null, "logo": null, "status": "Active",
  "challengeId": 1,
  "challenge": {
    "challengeId": 1, "title": "Climate Watch", "description": "...",
    "source": "NASA", "season": "2026", "image": null
  },
  "project": {
    "title": "Orbit", "description": "A satellite tracker.",
    "repositoryLink": "https://github.com/...", "demoLink": "https://..."
  },
  "students": [
    { "studentId": 1, "name": "Aisha Ali", "status": "Active" }
  ]
}
```
Errors: `team.not_found`.

### 7.3 `GET /teams/{teamId}/students` - `StaffOnly`
`200` → array of:
```json
{ "studentId": 1, "name": "Aisha Ali", "email": "aisha@example.com", "contact": "555-1001", "status": "Active" }
```

### 7.4 `PATCH /teams/{teamId}/logo` - `AdminOnly`
Body: `{ "image": "<base64|data-uri|url|null>" }`. `200` → updated
`TeamDetailResponse`. Errors: `team.not_found`, `image.invalid_type`,
`image.invalid_base64`, `image.too_large`.

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
  "sponsorId": 1, "name": "NASA Partner", "logo": "data:image/png;base64,...", "link": "https://...",
  "tierId": 1, "tier": { "tierId": 1, "name": "Tier 1", "priority": 1, "bannerEnabled": true },
  "active": true, "displayOrder": 1
}
```
`logo` is a base64 image, a data URI, or a URL (§1.1).

### 10.2 `GET /sponsors/{sponsorId}` - Public
`200` → one `SponsorResponse`. Errors: `sponsor.not_found`.

### 10.3 `POST /sponsors` - `AdminOnly`
Body:
```json
{ "name": "NASA Partner", "logo": "data:image/png;base64,...", "link": "https://...", "tierId": 1, "active": true, "displayOrder": 1 }
```
`logo` (base64/data-URI/URL, §1.1), `link`, `displayOrder` optional/nullable.
The returned sponsor's `tier` is `null` (created/updated responses omit the
nested tier). Errors: `sponsor.invalid`, `sponsor.tier_not_found`,
`image.invalid_type`, `image.invalid_base64`, `image.too_large`.

### 10.4 `PUT /sponsors/{sponsorId}` - `AdminOnly`
Same body & response as create. Errors: `sponsor.not_found`, `sponsor.invalid`,
`sponsor.tier_not_found`, plus the `image.*` errors.

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

Judging is **article-based**: each challenge has judging **criteria**, and each
criterion has a list of **article/reference questions** (plain prompts the judge
reads — no answers, no auto-grading). Judges score each question per team.

Each judge can submit and **publish their own** scores for a criterion
independently. The live board averages the scores of every judge who has
published that criterion. Only published-by-judge scores count; unpublished
judges' scores are hidden until they publish. Re-submitting scores replaces the
previous scores for the same judge, team, and question. See
[`JUDGING_PROCESS.md`](JUDGING_PROCESS.md) for a non-technical walkthrough.

### 12.1 `GET /judging/criteria?challengeId=` - Public
`200` → array of:
```json
{
  "criterionId": 1, "challengeId": 1, "name": "Team Work",
  "description": "How well the team collaborated and delivered together.", "maxScore": 10, "weight": 30,
  "isPublished": true, "publishedAt": "2026-10-22T10:00:00Z",
  "publishedJudgeCount": 1, "isPublishedByCaller": true,
  "questions": [
    { "criterionQuestionId": 1, "criterionId": 1, "text": "Did every team member contribute...", "displayOrder": 1, "isActive": true }
  ]
}
```
`publishedJudgeCount` is how many judges have published this criterion.
`isPublishedByCaller` is `null` for anonymous callers and for non-judge authenticated callers.

### 12.2 `POST /judging/criteria` - `AdminOnly`
```json
{
  "name": "Innovation", "description": "Novelty and creativity.",
  "maxScore": 10, "weight": 30, "challengeId": 1,
  "questions": [
    { "text": "How original is the concept?", "displayOrder": 1 },
    { "text": "Does the team explain the problem clearly?", "displayOrder": 2 }
  ]
}
```
`name` required; `maxScore > 0`; `challengeId` optional but must exist.
`200` → created `JudgingCriterionResponse`. Errors: `criterion.invalid`,
`challenge.not_found`.

### 12.3 `PUT /judging/criteria/{criterionId}` - `AdminOnly`
Same body/response. When `questions` is supplied it **replaces** the existing
question list. Errors: `criterion.not_found`, `criterion.invalid`, `challenge.not_found`.

### 12.4 `DELETE /judging/criteria/{criterionId}` - `AdminOnly`
`200` → `{ "id": 1, "deleted": true }`. Deleting a criterion also removes its
questions and scores. Errors: `criterion.not_found`.

### 12.5 `POST /judging/criteria/{criterionId}/publish` - `JudgeOrAdminOnly`
`POST .../unpublish` reverses it. `200` → updated `JudgingCriterionResponse`.

- **Judge:** publishes (or withdraws) their **own** scores only.
- **Admin:** may pass `?judgeId=N` to target a specific judge, or omit the
  parameter to **force-publish every judge who has scored** the criterion.
- Publishing is idempotent; scores only appear once their judge publishes.
- Side effect: broadcasts `StandingsUpdated`. Errors: `criterion.not_found`,
  `judging.not_allowed`, `judging.no_scores`.

### 12.6 `GET /judging/standings` - Public
The single source of truth for the live board. **Ranked by achieved percentage**
(`achievedScore / availableScore * 100`). Only scores from **published-by-judge**
entries are counted, and each criterion is **averaged across the judges who
published it**. A skipped question is excluded for the judge who skipped it but
still counts for others. Per-question detail is not exposed. `200` → array of:
```json
{
  "rank": 1, "isTied": false, "teamId": 1, "teamName": "Orion",
  "achievedScore": 8.5, "availableScore": 10.0, "percentage": 85.0,
  "judgesCounted": 2,
  "criteria": [
    { "criterionId": 1, "criterionName": "Team Work", "achievedScore": 8.5, "availableScore": 10.0, "judgesCounted": 2 }
  ]
}
```
`rank` is `null` when the team has no published, scored criteria yet.
`judgesCounted` shows how many judges contributed (useful when coverage is uneven).

### 12.7 `POST /judging/scores` - `JudgeOnly`
`judgeId` comes from the token - **never** send it in the body. Body:
```json
{
  "teamId": 1,
  "scores": [
    { "criterionId": 1, "criterionQuestionId": 1, "name": null, "maxScore": 5, "score": 4, "comment": "Clear idea" },
    { "criterionId": 1, "criterionQuestionId": null, "name": "Bonus: teamwork", "maxScore": 5, "score": 5 },
    { "criterionId": 1, "criterionQuestionId": null, "name": "Off-topic ask", "maxScore": 5, "score": 0, "skipped": true }
  ]
}
```
Rules:
- For a catalogue question, send `criterionQuestionId` (it must belong to
  `criterionId`); `maxScore` defaults to the criterion's `maxScore`.
- For an ad-hoc **"other"** question, leave `criterionQuestionId` null and give a
  `name` (optional) and an explicit `maxScore`.
- `skipped: true` excludes that entry from the team's result for this judge only
  (it counts neither for nor against this judge).
- Otherwise `0 ≤ score ≤ maxScore`.
- **Upsert:** re-submitting the same judge, team, criterion, and question
  replaces the existing score (no duplicates).

`200` → array of `QuestionScoreResponse`:
```json
{ "questionScoreId": 1, "teamId": 1, "criterionId": 1, "criterionQuestionId": 1,
  "questionName": "How original is the concept?", "maxScore": 5, "score": 4,
  "skipped": false, "comment": "Clear idea", "timestamp": "2026-10-04T09:30:00Z" }
```
Side effects: broadcasts `ScoresSubmitted` then `StandingsUpdated`. Errors:
in-code `403 judging.no_judge_link`, `team.not_found`, `judging.no_scores`,
`criterion.not_found`, `criterion_question.not_found`, `score.invalid`.

### 12.8 `GET /judging/scores?teamId=` - `StaffOnly`
`200` → array of `QuestionScoreResponse` (per-question detail). `teamId` required.

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
  "role": "Admin", "memberType": null, "photo": null, "status": "Active", "judgeId": null,
  "createdAt": "2026-10-04T09:30:00Z"
}
```
`photo` is the core-team member's picture (base64/data-URI/URL, §1.1).

### 14.2 `GET /staff/{staffId}` - `AdminOnly`
`200` → one `StaffResponse`. Errors: `staff.not_found`.

### 14.3 `POST /staff` - `AdminOnly`
```json
{
  "fullName": "Sara Hassan", "username": "sara.mentor", "email": "sara.mentor@nasa.local",
  "password": "Mentor123!", "role": "CoreTeam", "memberType": "Mentor",
  "photo": "data:image/jpeg;base64,..."
}
```
Rules: `fullName`/`username`/`email` required; `password` required, ≥ 6 chars;
`role` ∈ Admin/CoreTeam/Judge; CoreTeam requires a valid `memberType`; `photo`
optional (base64/data-URI/URL, §1.1). Creating a Judge auto-creates a linked
Judge profile and returns a non-null `judgeId`.
Errors: `staff.invalid`, `staff.invalid_role`, `staff.weak_password`,
`staff.username_taken`, `staff.email_taken`, `staff.member_type_required`,
`staff.invalid_member_type`, `image.invalid_type`, `image.invalid_base64`,
`image.too_large`.

### 14.4 `PUT /staff/{staffId}` - `AdminOnly`
```json
{
  "fullName": "Sara Hassan", "username": "sara.mentor", "email": "sara@nasa.local",
  "password": null, "role": "CoreTeam", "memberType": "Mentor", "status": "Active",
  "photo": "data:image/jpeg;base64,..."
}
```
`password` is optional (omit/`null` to keep it; ≥ 6 chars if provided).
`status` is optional; only `Active`/`Inactive` are accepted (other values are
ignored). `photo` replaces the picture (send `null`/`""` to clear). Changing the
role to `Judge` auto-creates a linked judge; changing away from `Judge` clears
the link. Errors: `staff.not_found`, plus the create errors.

### 14.4b Status shortcuts
- `PATCH /staff/{staffId}/deactivate` - sets `Inactive` (no login thereafter).
- `PATCH /staff/{staffId}/activate` - sets `Active`.
Both return the updated `StaffResponse`. Errors: `staff.not_found`.

### 14.5 `DELETE /staff/{staffId}` - `AdminOnly`
Hard delete. `200` → `{ "id": 1, "deleted": true }`. Errors: `staff.not_found`.

### 14.6 `GET /staff/member-types` - `AdminOnly`
`200` → `["Organizer","Mentor","Developer","Marketing","PR","Media","Highboard"]`.

---

## 14b. Season media (landing page)

Past-season photos/footage shown on the public site.

### 14b.1 `GET /seasonmedia?includeInactive=false` - Public
`200` → array of:
```json
{
  "mediaId": 1, "season": "2025", "title": "Global Hackathon Highlights",
  "description": "Footage from the 2025 season.", "image": "data:image/png;base64,...",
  "link": "https://example.com/2025-highlights", "mediaType": "video",
  "displayOrder": 1, "active": true
}
```
`image` is base64/data-URI/URL (§1.1); `link` is an optional external URL;
`mediaType` is free-form (`image`, `video`, ...).

### 14b.2 `GET /seasonmedia/{mediaId}` - Public
`200` → one item. Errors: `season_media.not_found`.

### 14b.3 `POST /seasonmedia` - `AdminOnly`
```json
{
  "season": "2025", "title": "Global Hackathon Highlights", "description": "...",
  "image": "data:image/png;base64,...", "link": "https://...", "mediaType": "video",
  "displayOrder": 1, "active": true
}
```
`season` and `title` required. `200` → created item. Errors: `season_media.invalid`,
`image.invalid_type`, `image.invalid_base64`, `image.too_large`.

### 14b.4 `PUT /seasonmedia/{mediaId}` - `AdminOnly`
Same body & response as create. Errors: `season_media.not_found`, `season_media.invalid`, `image.*`.

### 14b.5 `DELETE /seasonmedia/{mediaId}` - `AdminOnly`
`200` → `{ "id": 1, "deleted": true }`. Errors: `season_media.not_found`.

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
| Judging | `ScoresSubmitted` | `{ teamId, judgeId, scores: QuestionScoreResponse[] }` |
| Judging | `StandingsUpdated` | full standings array (`StandingsEntryResponse[]`, averaged across published judges) |

`RequestsHub` client calls:
- `JoinOrganizers()` - staff only; adds the caller to the `Organizers` group.
- `LeaveOrganizers()`.

---

## 16. Seed accounts (local / dev only)

Staff (login via `/auth/login/staff`): `admin`/`Admin123!`, `coreteam`/`Core123!`,
`sara.mentor`/`Mentor123!`, `omar.dev`/`Dev123!`, `judge`/`Judge123!`.

Teams (login via `/auth/login/participant` with their username): `orion` and
`atlas`, password `Password123!`.

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
