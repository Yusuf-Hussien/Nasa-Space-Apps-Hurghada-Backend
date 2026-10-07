# MOBILE API CONTRACT (Team App - Flutter)

## Overview
- Base URL: /api
- Auth: Team-based JWT/Token (shared team credentials). Roles context: team member.
- Content-Type: application/json; Bearer token for protected routes.
- Error model: { ""error"": { ""code"": string, ""message"": string, ""details""?: any }, ""timestamp"": datetime }
- Validation: Server-side on all inputs.

## 1. AUTHENTICATION
### POST /api/auth/login (team)
- Purpose: Authenticate with shared team username/password. Returns team context + token.
- Request:
`json
{ ""username"": ""string"", ""password"": ""string"" }
`
- Response 200:
`json
{
  ""token"": ""string"",
  ""token_type"": ""Bearer"",
  ""expires_in"": 3600,
  ""team"": { ""teamId"": 1, ""name"": ""string"", ""username"": ""string"", ""challengeId"": 1, ""status"": ""active"" }
}
`
- Notes: Never return password_hash. Use secure hashing; do not log credentials.

## 2. TEAM CONTEXT
### GET /api/teams/me
- Auth: team token
- Purpose: Return authenticated team's full context.
- Response 200:
`json
{
  ""teamId"": 1,
  ""name"": ""string"",
  ""challengeId"": 1,
  ""challenge"": { ""challengeId"": 1, ""title"": ""string"", ""summary"": ""string"", ""season"": ""string"", ""isPublished"": true },
  ""students"": [{ ""studentId"": 1, ""fullName"": ""string"", ""email"": ""string?"", ""contactNumber"": ""string?"", ""status"": ""active"" }]
}
`

## 3. SERVICE REQUESTS (Organizer/Mentor)
### POST /api/requests
- Auth: team token
- Request:
`json
{ ""requestType"": ""organizer|mentor"", ""message"": ""string (max 2000)"", ""location"": ""string (max 200)"" }
`
- Response 201:
`json
{
  ""requestId"": 1,
  ""teamId"": 1,
  ""requestType"": ""organizer|mentor"",
  ""message"": ""string?"",
  ""location"": ""string?"",
  ""status"": ""pending"",
  ""createdAt"": ""datetime""
}
`

### GET /api/requests/my
- Auth: team token
- Query: ?status=pending|accepted|resolved|cancelled&limit=20&offset=0
- Response 200:
`json
{
  ""items"": [{
    ""requestId"": 1,
    ""requestType"": ""organizer|mentor"",
    ""status"": ""pending|accepted|resolved|cancelled"",
    ""message"": ""string?"",
    ""location"": ""string?"",
    ""createdAt"": ""datetime"",
    ""resolvedAt"": ""datetime?""
  }],
  ""total"": 1
}
`

## 4. CHALLENGE QUESTIONS / PRIZE GAME
### GET /api/game/questions
- Auth: team token
- Query: ?challengeId={team.challengeId}&type=game|evaluation
- Response 200:
`json
{
  ""items"": [{
    ""questionId"": 1,
    ""challengeId"": 1,
    ""questionType"": ""evaluation|game"",
    ""questionText"": ""string"",
    ""optionsJson"": [] | null,
    ""isActive"": true
  }]
}
`
- Note: correctAnswer never returned to mobile.

### POST /api/game/answers
- Auth: team token
- Request: { ""questionId"": 1, ""answerText"": ""string (max 400)"" }
- Response 201: { ""participationId"": 1, ""questionId"": 1, ""teamId"": 1, ""isCorrect"": true, ""submittedAt"": ""datetime"" }

## 5. SPONSORS / TIER 1 BANNER
### GET /api/sponsors/banner
- Auth: optional
- Query: ?tier=1&active=true
- Response 200:
`json
{
  ""items"": [{
    ""sponsorId"": 1,
    ""name"": ""string"",
    ""logoUrl"": ""string?"",
    ""websiteUrl"": ""string?"",
    ""tierId"": 1,
    ""displayOrder"": 0,
    ""isActive"": true
  }]
}
`

## 6. STANDINGS
### GET /api/standings
- Auth: optional
- Query: ?limit=20
- Response 200:
`json
{
  ""items"": [{ ""teamId"": 1, ""teamName"": ""string"", ""totalScore"": 0.0000 }],
  ""updatedAt"": ""datetime""
}
`

## Notes
- Real-time: SignalR hubs /hubs/standings, /hubs/requests (team-{teamId}, organizer scope) for live updates.
- Team-only auth; no individual participant accounts.
