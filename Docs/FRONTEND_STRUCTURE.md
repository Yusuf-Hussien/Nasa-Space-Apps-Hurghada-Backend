# Frontend Structure Specification: NASA Space Apps Hurghada Platform

**Version:** 1.0  
**Date:** 2026-10-22  
**Backend API:** REST \pi/v1/*\ + SignalR (\/hubs/requests\, \/hubs/judging\)  
**Auth:** JWT Bearer tokens (role-based)  
**References:** \Docs/API_CONTRACT.md\, \Docs/JUDGING_PROCESS.md\, \Docs/PRD_TRACEABILITY.md\, \README.md\

---

## 1) Goals
Define a concrete, implementation-ready frontend structure (routes, components, state, RBAC, API mapping, real-time, and UX flows) that matches the backend exactly for handoff to the frontend team.

---

## 2) Tech Stack Recommendations
| Layer | Suggestion | Notes |
|---|---|---|
| Framework | React + TypeScript + Vite | Matches existing setup |
| Routing | React Router v6+ | Role-based guards |
| Server State | React Query (TanStack Query) | Caching, invalidation, retries |
| Global State | React Context or Zustand | Auth/session, real-time connections |
| HTTP | Axios | Interceptors for auth + error envelope parsing |
| Forms | React Hook Form + Zod | Validation |
| Real-time | @microsoft/signalr | Hubs with auto-reconnect |
| UI | shadcn/ui (or MUI/Tailwind) | Consistent components |
| Utils | date-fns, clsx, tailwind-merge | As needed |

---

## 3) Project Structure
`	ext
src/
├── app/                # Entry, router, providers, layout
├── config/             # env, constants, API base URLs
├── features/           # Feature modules (auth, judging, teams, attendance, etc.)
├── shared/             # UI, hooks, utils, types, api client, guards
└── assets/             # Static assets
`

---

## 4) Pages Structure (Complete)

### 4.1 Public Pages (No Auth Required)
| Route | Page Name | Purpose | Key APIs | Notes |
|---|---|---|---|---|
| / | Landing/Home | Event overview, highlights, sponsors preview | GET /challenges, GET /sponsors, GET /seasonmedia | Entry point |
| /login | Login Selector | Choose Staff or Participant login | - | Role selection |
| /login/staff | Staff Login | Staff login form | POST /auth/login/staff | identifier (username/email) + password, rememberMe |
| /login/participant | Participant Login | Team login form | POST /auth/login/participant | Team username + password, rememberMe |
| /standings | Public Standings | Live scoreboard | GET /judging/standings + SignalR StandingsUpdated | Ranked by %, averaged across published judges, shows judgesCounted, ties |
| /teams | Teams List | Browse all teams | GET /teams?search= | username hidden unless staff |
| /teams/:teamId | Team Detail (Public) | Team + full challenge + project + student names only | GET /teams/:teamId | No email/contact for public; username null unless staff |
| /challenges | Challenges List | All challenges | GET /challenges | - |
| /challenges/:challengeId | Challenge Detail | Challenge details + image | GET /challenges/:challengeId | - |
| /sponsors | Sponsors | Sponsors grouped by tier | GET /sponsors, GET /sponsors/tiers | - |
| /season-media | Season Media Gallery | Past season media | GET /seasonmedia | Public (images/videos/links) |
| /leaderboard/game | Game Leaderboard | Side trophy leaderboard | GET /game/leaderboard | Public |
| / (Core Team section) | Core Team Section | Active CoreTeam members on the homepage | GET /core-team | Public; returns only staffId, fullName, memberType, photo |

### 4.2 Authenticated Pages (Any Role)
| Route | Page Name | Purpose | Key APIs | Notes |
|---|---|---|---|---|
| /me | Profile | Current user info | GET /auth/me | Role-specific shape |
| /account/change-password | Change Password | Update own password | POST /auth/change-password | Any authenticated role; current + new password |
| /requests | Service Requests | List/create/delete requests | GET /requests, POST /requests, DELETE /requests/:requestId | Participant scoped to own team; staff see all |
| /questions | Questions Bank | Browse questions | GET /questions, GET /questions/:questionId | Answers stripped for participants, full for staff |

#### 4.2.1 Change Password (Web + Mobile)
- **Web route:** `/account/change-password` under `features/auth`; link it from the profile menu for every authenticated role.
- **Mobile screen:** `ChangePasswordScreen` in the account/settings area, reachable from the participant dashboard and staff profile.
- **Fields:** current password, new password, confirm new password. Confirmation is client-side only and is not sent.
- **API:** `POST /api/v1/auth/change-password` with the bearer token.
- **Body:** `{ "currentPassword": "...", "newPassword": "..." }`.
- **Client validation:** new password at least 6 characters and different from the current password.
- **Success:** show a success message, clear the form, and keep the current session. The access token is not reissued and remains valid until expiry.
- **Errors:** map `auth.invalid_current_password`, `auth.weak_password`, and `auth.password_unchanged` to form errors without logging out. On `auth.account_inactive`, `user.not_found`, or a normal `401`, clear the session and redirect to login.
- **Do not:** send a user/team/staff id in the body; the backend derives the account from the token.

### 4.3 Staff Pages (Admin/CoreTeam/Judge)
| Route | Page Name | Purpose | Key APIs | Notes |
|---|---|---|---|---|
| /staff/teams/:teamId/students | Team Roster (Staff) | Full student details | GET /teams/:teamId/students | Includes email/contact (hidden publicly) |
| /staff/requests | Requests Management | Resolve service requests | PATCH /requests/:requestId | StaffOnly |
| /staff/judging/scores | Criterion Score Detail | View all criterion scores per team | GET /judging/scores?teamId= | StaffOnly - one row per judge + criterion |
| /staff/attendance | Attendance Records | Filter attendance records | GET /attendance?teamId=&eventDate=&status= | StaffOnly |

### 4.4 CoreTeam Pages
| Route | Page Name | Purpose | Key APIs | Notes |
|---|---|---|---|---|
| /coreteam/attendance/check-in | Attendance Check-In | Mark attendance by team/student | GET /attendance/teams?search=&eventDate=, POST /attendance | CoreTeamOnly, bulk upsert |

### 4.5 Judge Pages
| Route | Page Name | Purpose | Key APIs | Notes |
|---|---|---|---|---|
| /judge/dashboard | Judge Dashboard | Teams list + criteria status | GET /teams, GET /judging/criteria | See which criteria published by self (isPublishedByCaller) |
| /judge/teams/:teamId/score | Team Scoring Interface | One 1.0-5.0 score + optional comment per global criterion, then publish | GET /judging/criteria, POST /judging/scores, POST /judging/criteria/:criterionId/publish | Upsert on resubmit; publish own scores only; no per-question/ad-hoc/skip |
| /judge/my-publications | My Publications | Track published criteria | GET /judging/criteria | Check isPublishedByCaller and publishedJudgeCount |

### 4.6 Admin Pages
| Route | Page Name | Purpose | Key APIs | Notes |
|---|---|---|---|---|
| /admin/dashboard | Admin Overview | Quick stats/overview | Aggregate endpoints | AdminOnly |
| /admin/staff | Staff Management | CRUD staff, roles, member types | GET/POST/PUT/DELETE /staff, GET /staff/member-types | AdminOnly, judge auto-linked on create/update to Judge |
| /admin/criteria | Criteria Management | CRUD global criteria + optional reference prompts | GET/POST/PUT/DELETE /judging/criteria | AdminOnly; maxScore/weight locked at 5/20 |
| /admin/criteria/publish | Publish Management | Force-publish/target judges | GET /judging/criteria, POST /judging/criteria/:criterionId/publish?judgeId=, GET /judging/scores?teamId= | AdminOnly - omit judgeId to force-publish all judges with scores |
| /admin/sponsors | Sponsors Management | CRUD sponsors/tiers + logos | GET/POST/PUT/DELETE /sponsors, GET/POST/PUT/DELETE /sponsors/tiers | AdminOnly, base64/data URI/URL (20MB decoded) |
| /admin/questions | Game Questions Management | Start/stop game + CRUD + publish/unpublish | GET /game/state, POST /game/start, POST /game/stop, GET/POST/PUT/DELETE /questions, POST /questions/:questionId/publish, POST /questions/:questionId/unpublish | AdminOnly; start game first, then publish individual questions to reveal them |
| /admin/season-media | Season Media Management | CRUD season media | GET/POST/PUT/DELETE /seasonmedia | AdminOnly |
| /admin/email | Bulk Email | Compose subject/body and send to many recipients | POST /email/send | AdminOnly; branded footer/signature is added automatically; plain text by default, set isBodyHtml for HTML |
| /admin/challenges | Challenges Management | Set challenge images | PATCH /challenges/:challengeId/image | AdminOnly |
| /admin/teams | Teams Management | Set team logos | PATCH /teams/:teamId/logo | AdminOnly |

### 4.7 Participant Pages
| Route | Page Name | Purpose | Key APIs | Notes |
|---|---|---|---|---|
| /participant/dashboard | Participant Dashboard | Team info, challenge, project | GET /auth/me or GET /teams/:teamId | ParticipantOnly |
| /participant/game | Game Play | Wait for admin start, then answer questions | GET /game/state, GET /questions, POST /game/questions/:questionId/answer | Show a waiting screen when `isStarted` is false; questions are hidden until both started and published; one answer per team per question |
| /participant/requests | My Requests | Create/manage own requests | GET/POST/DELETE /requests | Auto-scoped to own team |

---

## 5) Judging Model (Frontend Must Follow)
- **Global criteria:** The same five criteria apply to every team: Impact, Creativity, Validity, Relevance, Presentation. Criteria are not challenge-scoped.
- **Criterion-level only:** One decimal score from 1.0 to 5.0 plus an optional comment per criterion. No question-level, ad-hoc, or skip controls.
- **Reference prompts:** `questions` are optional reading guidance only and are never scored.
- **Per-judge publish:** Each judge publishes their own criterion scores independently.
- **Averaging:** Standings average scores across judges who published (achieved/available per criterion averaged, summed).
- **Visibility:** Unpublished judges' scores do not appear until published.
- **Upsert:** Resubmitting POST /judging/scores replaces the same judge+team+criterion.
- **Transparency:** Display judgesCounted per entry and per criterion; show isPublishedByCaller/publishedJudgeCount.
- **Publishing:** Judges use POST /judging/criteria/:id/publish (own). Admin may omit judgeId (force all with scores) or pass ?judgeId=N.
- **Weight:** Weight is display-only and does not change ranking.

---

## 6) RBAC & Guards
| Policy | Roles | Usage |
|---|---|---|
| AdminOnly | Admin | Staff/criteria/sponsors/questions/season-media/challenges/teams management, force-publish |
| CoreTeamOnly | CoreTeam | Attendance check-in |
| JudgeOnly | Judge | Submit scores |
| JudgeOrAdminOnly | Judge, Admin | Publish/unpublish criterion scores |
| StaffOnly | Admin, CoreTeam, Judge | Roster, requests resolve, attendance list, score detail |
| ParticipantOnly | Participant | Game answers, own requests |

---

## 7) Real-Time
- /hubs/judging (public/anonymous allowed): ScoresSubmitted, StandingsUpdated → refresh standings/live UI
- /hubs/requests (authenticated): join Organizers for staff; RequestCreated/Updated/Deleted

---

## 8) Images & Errors
- **Images:** base64/data URI/URL; max 20MB decoded; mime allowlist png/jpeg/jpg/webp/gif/svg+xml. Errors: image.invalid_type, image.invalid_base64, image.too_large
- **Error envelope:** { "error": { "code", "message" } }

---

## 9) Implementation Checklist
- [ ] Setup router + guards + providers
- [ ] API client + auth interceptor
- [ ] Auth flows (staff/participant)
- [ ] Change-password screen (web + mobile)
- [ ] Public pages + standings (SignalR)
- [ ] Judge scoring + per-judge publish
- [ ] Admin criteria + publish management
- [ ] Staff operations (requests/attendance/scores)
- [ ] CoreTeam check-in
- [ ] Participant dashboard/game
- [ ] Sponsors/season-media/challenges/teams management
- [ ] Error handling + toasts + loading states
