# Judging Process Guide

This guide explains how judging works at a NASA Space Apps event on this
platform. It is written for organizers, mentors, and judges - no technical
knowledge required.

---

## 1. The big picture

Every team is judged against the same five **global criteria**:

1. **Impact**
2. **Creativity**
3. **Validity**
4. **Relevance**
5. **Presentation**

A judge gives each team **one score per criterion**, from **1.0 to 5.0**.
Decimals are allowed. Nothing is automatically graded.

```
Global criteria
  - Impact       -> one score per team
  - Creativity   -> one score per team
  - Validity     -> one score per team
  - Relevance    -> one score per team
  - Presentation -> one score per team
```

Criteria are not tied to a challenge. Judges may optionally read reference
prompts under a criterion, but those prompts are guidance only and are never
scored.

---

## 2. The roles

| Role | What they do |
|---|---|
| **Admin** | Manages criteria, optional reference prompts, and can publish on a judge's behalf |
| **CoreTeam** (organizers) | Runs the event; can read detailed scores |
| **Judge** | Reviews teams and submits one score per criterion |
| **Team (Participant)** | Builds the project; can see the public scoreboard |

Only a **Judge** can submit scores. Each judge submits and publishes their own
scores. An **Admin** can force-publish or target a specific judge.

---

## 3. Setting up the judging form

On a fresh event the platform seeds the five criteria automatically, with a
maximum score of **5** for each. An **Admin** can:

1. Edit a criterion's name and description.
2. Add optional reference prompts to guide judges. These are not scored and may
   be left empty.
3. Reorder or remove reference prompts.

The maximum score and weight are locked at **5** and **20**. Weight is shown for
reference only and does not change the standings.

---

## 4. How a judge scores a team

For each team, a judge:

1. Opens the team and reviews the project.
2. Gives **one score from 1.0 to 5.0 for each criterion**.
3. Optionally leaves a short comment per criterion.

There are no question-level scores, ad-hoc scores, or skips. Every criterion
receives exactly one score per judge per team.

### Re-scoring

If a judge submits again for the same team and criterion, the new score
**replaces** the old one and is published again for that team.

---

## 5. Publishing and the live scoreboard

Submitting a score **publishes it immediately** for that judge, team, and
criterion, so the public board updates as soon as the judge submits.

- Judges submit and publish their **own** score for one team at a time.
- Judges can unpublish a criterion for a team to withdraw their score, then
  submit again to republish it.
- An Admin can publish or withdraw scores for every scored judge/team pair, or
  narrow the change by judge, team, or both.
- Publishing is idempotent and is tracked per judge, team, and criterion.

The scoreboard only counts published scores.

### How ranking works

For each team, the board:

1. Averages each criterion's achieved score and maximum score across the judges
   who published that criterion.
2. Sums the averaged criteria into a team total.
3. Ranks teams by **achieved / available x 100**.

Unpublished scores are ignored. Teams with no published scores appear without a
rank. Tied percentages share the same rank, and the board shows how many judges
contributed.

---

## 6. Quick checklist

**Admin**
- [ ] Review the seeded criteria and descriptions.
- [ ] Optionally add reference prompts.
- [ ] Confirm judges can log in.

**Judge**
- [ ] Review each team.
- [ ] Give one 1.0-5.0 score per criterion.
- [ ] Add comments where useful.
- [ ] Submit each criterion; scores publish automatically.

**Everyone**
- [ ] Watch the public standings update live.
