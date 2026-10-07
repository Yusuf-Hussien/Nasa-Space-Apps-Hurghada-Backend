# WEB API CONTRACT (React - Public, Judges, Organizers)

## Overview
- Base URL: /api
- Roles: Public, Judge, Organizer, Mentor, Admin (as per PRD). Auth via JWT.
- Content-Type: application/json; Bearer for protected routes.
- Error model: { ""error"": { ""code"": string, ""message"": string, ""details""?: any }, ""timestamp"": datetime }
- Validation: Server-side.

## 1. PUBLIC (Landing, Sponsors, Standings)
From web screens: home-dasboard.jpeg, challanges-dashboard.jpeg, live-standing-dashboard.jpeg.

### GET /api/public/event
- Auth: none
- Purpose: Event overview, season, core team, past seasons media (2024/2025).
- Response 200: { ""event"": { ""name"": ""NASA Space Apps"", ""season"": ""string"", ""description"": ""string"" }, ""coreTeam"": [{ ""name"": ""string"", ""role"": ""string"" }], ""media"": [{ ""type"": ""image|video"", ""url"": ""string"", ""year"": 2024|2025 }] }

### GET /api/challenges
- Auth: none
- Query: ?season=string&published=true
- Response 200: { ""items"": [{ ""challengeId"": 1, ""title"": ""string"", ""summary"": ""string"", ""source"": ""string"", ""season"": ""string"", ""isPublished"": true, ""metadataJson"": any|null }] }

### GET /api/sponsors
- Auth: none
- Query: ?active=true&tier=1|2|3
- Response 200: { ""items"": [{ ""sponsorId"": 1, ""name"": ""string"", ""logoUrl"": ""string?"", ""websiteUrl"": ""string?"", ""tierId"": 1, ""displayOrder"": 0, ""isActive"": true, ""tier"": { ""tierId"": 1, ""name"": ""string"", ""priority"": 1, ""bannerEnabled"": true } }] }

### GET /api/standings
- Auth: none
- Query: ?limit=50
- Response 200: { ""items"": [{ ""teamId"": 1, ""teamName"": ""string"", ""totalScore"": 0.0000, ""rank"": 1, ""projectTitle"": ""string?"" }], ""updatedAt"": ""datetime"" }
- Real-time: SignalR /hubs/standings.

## 2. JUDGE AUTH & INTERFACE
From: judges-login-dashboard.jpeg, judge-dashboard.jpeg, team-preview-for-judge.jpeg.

### POST /api/auth/login (judge)
- Auth: none
- Request: { ""accountName"": ""string"", ""password"": ""string"" } (or username)
- Response 200: { ""token"": ""string"", ""expires_in"": 3600, ""judge"": { ""judgeId"": 1, ""fullName"": ""string"", ""accountName"": ""string"", ""role"": ""judge"", ""isActive"": true } }

### GET /api/judging/criteria
- Auth: judge/organizer/admin
- Query: ?active=true
- Response 200: { ""items"": [{ ""criterionId"": 1, ""name"": ""string"", ""summary"": ""string?"", ""maxScore"": 100.0000, ""weight"": 0.2500, ""isActive"": true }] }

### GET /api/teams
- Auth: judge/organizer/admin
- Purpose: List/search participating teams.
- Query: ?search=string (teamId or username) & challengeId=int & status=active
- Response 200: { ""items"": [{ ""teamId"": 1, ""name"": ""string"", ""username"": ""string"", ""challengeId"": 1, ""status"": ""active"", ""project"": { ""projectId"": 1, ""title"": ""string?"", ""summary"": ""string?"", ""repositoryUrl"": ""string?"", ""demoUrl"": ""string?"" }, ""challenge"": { ""challengeId"": 1, ""title"": ""string"" } }] }

### GET /api/teams/{teamId}
- Auth: judge/organizer/admin
- Response 200: includes students, project, challenge, existing scores by judge.

### GET /api/teams/{teamId}/students
- Auth: judge/organizer/admin
- Response 200: { ""items"": [{ ""studentId"": 1, ""fullName"": ""string"", ""email"": ""string?"", ""contactNumber"": ""string?"", ""status"": ""active"" }] }

### GET /api/challenges/{challengeId}/questions
- Auth: judge/organizer/admin
- Purpose: Potential evaluation questions to assist judging.
- Query: ?type=evaluation
- Response 200: { ""items"": [{ ""questionId"": 1, ""questionType"": ""evaluation"", ""questionText"": ""string"", ""optionsJson"": any|null, ""isActive"": true }] }

### POST /api/judging/scores
- Auth: judge
- Purpose: Submit score for (teamId, criterionId). Enforce unique (judgeId, teamId, criterionId). Validate score range (0..maxScore).
- Request:
`json
{
  ""teamId"": 1,
  ""criterionId"": 1,
  ""score"": 95.0000,
  ""comment"": ""string (max 2000)?""
}
`
- Response 201: { ""scoreId"": 1, ""teamId"": 1, ""judgeId"": 1, ""criterionId"": 1, ""score"": 95.0000, ""comment"": ""string?"", ""recordedAt"": ""datetime"" }
- Side effect: trigger standings recalculation + SignalR update.

### PUT/PATCH /api/judging/scores/{scoreId}
- Auth: judge (owner) or admin
- Request: { ""score"": 95.0000, ""comment"": ""string?"" }
- Response 200: updated score record.

## 3. ORGANIZER INTERFACE
From: organziers-requests.jpeg, organziers-takes-attendance for teams.jpeg.

### GET /api/organizer/requests
- Auth: organizer/mentor/admin
- Query: ?requestType=organizer|mentor&status=pending|accepted|resolved|cancelled&limit=50&offset=0
- Response 200:
`json
{
  ""items"": [{
    ""requestId"": 1,
    ""teamId"": 1,
    ""teamName"": ""string"",
    ""requestType"": ""organizer|mentor"",
    ""message"": ""string?"",
    ""location"": ""string?"",
    ""status"": ""pending|accepted|resolved|cancelled"",
    ""createdAt"": ""datetime"",
    ""resolvedAt"": ""datetime?""
  }],
  ""total"": 1
}
`
- Real-time: new requests appear via /hubs/requests.

### PATCH /api/organizer/requests/{requestId}
- Auth: organizer/mentor/admin
- Purpose: Update status.
- Request: { ""status"": ""accepted|resolved|cancelled"", ""resolvedAt""?: ""datetime"" }
- Rules: only allowed transitions; set resolvedAt when resolved/cancelled; audit actor.
- Response 200: updated request.

### GET /api/attendance/team/{teamId}
- Auth: organizer/admin
- Purpose: Get students + current attendance for team/date.
- Query: ?eventDate=YYYY-MM-DD (default today)
- Response 200: { ""teamId"": 1, ""students"": [{ ""studentId"": 1, ""fullName"": ""string"", ""status"": ""present|late|absent|not_marked"" }] }

### POST /api/attendance
- Auth: organizer/admin
- Purpose: Bulk mark attendance per student.
- Request:
`json
{
  ""eventDate"": ""YYYY-MM-DD"",
  ""records"": [
    { ""studentId"": 1, ""status"": ""present|late|absent"" }
  ]
}
`
- Response 201: { ""saved"": 2, ""eventDate"": ""YYYY-MM-DD"", ""recordedAt"": ""datetime"", ""markedBy"": ""string"" }
- Rules: unique (studentId, eventDate); prevent duplicates (upsert via UX + UX_attendances constraint).

## 4. QUESTIONS MANAGEMENT (Organizer/Admin)
### GET /api/questions
- Auth: organizer/admin
- Query: ?challengeId=int&type=evaluation|game&active=true
- Response 200: { ""items"": [{ ""questionId"": 1, ""challengeId"": 1, ""questionType"": ""evaluation|game"", ""questionText"": ""string"", ""optionsJson"": any|null, ""correctAnswer"": ""string?"", ""isActive"": true }] }

### POST /api/questions
- Auth: admin
- Request: { ""challengeId"": 1, ""questionType"": ""evaluation|game"", ""questionText"": ""string"", ""optionsJson"": any|null, ""correctAnswer"": ""string?"", ""isActive"": true }
- Response 201.

### PATCH /api/questions/{questionId}
- Auth: admin
- Request: partial fields
- Response 200.

## 5. GAME PARTICIPATIONS (Review)
### GET /api/game/participations
- Auth: organizer/admin
- Query: ?questionId=int&teamId=int&limit=50
- Response 200: { ""items"": [{ ""participationId"": 1, ""questionId"": 1, ""teamId"": 1, ""teamName"": ""string"", ""answerText"": ""string?"", ""isCorrect"": true, ""submittedAt"": ""datetime"" }] }

## 6. ADMIN/SYSTEM (minimal)
### POST /api/admin/teams
- Auth: admin
- Request: { ""name"": ""string"", ""username"": ""string"", ""password"": ""string"", ""challengeId"": 1, ""status"": ""active"" }
- Response 201: team (no password_hash).

### POST /api/admin/sponsors
- Auth: admin
- Request: { ""name"": ""string"", ""tierId"": 1, ""logoUrl"": ""string?"", ""websiteUrl"": ""string?"", ""isActive"": true, ""displayOrder"": 0 }
- Response 201.

## Notes
- Real-time: use SignalR for standings and organizer requests.
- Standing calculation uses judge_scores (by criteria weights/max) per PRD; tie-break rules configurable.
- Public standings viewable without auth. Judge/Org require auth.
- Server validates score range against judging_criteria.max_score.
