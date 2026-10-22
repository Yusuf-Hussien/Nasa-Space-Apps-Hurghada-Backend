# Judging Process Guide

This guide explains how judging works at a NASA Space Apps event on this
platform. It is written for organizers, mentors, and judges — no technical
knowledge required.

---

## 1. The big picture

Teams build a project for a **challenge**. To decide a winner, each challenge has
a set of **criteria** (for example *Innovation*, *Technical Execution*, and
*Impact*). Each criterion holds a short list of **questions** that a judge reads
while looking at the team's work and then scores.

Nothing is automatically graded in judging. The judge reviews the team's project
and judges it question by question, awarding points up to a maximum.

```
Challenge
  └── Criterion  (e.g. "Innovation")
        ├── Question 1  -> judge gives a score
        └── Question 2  -> judge gives a score
```

---

## 2. The roles

| Role | What they do |
|---|---|
| **Admin** | Sets up the criteria and the questions inside them |
| **CoreTeam** (organizers) | Runs the event; can also publish results and read detailed scores |
| **Judge** | Reads each team's work and submits scores |
| **Team (Participant)** | Builds the project; can see the public scoreboard |

Only a **Judge** can submit scores. Only **Admin** can create or edit criteria
and questions. **Admin** and **CoreTeam** together can publish a criterion to the
live board.

---

## 3. Setting up the judging form

Before the event (or judging round), an **Admin**:

1. Creates a criterion for the challenge and gives it a name, a short
   description, and a maximum score.
2. Adds the questions that belong to that criterion. Each question is just a
   prompt to guide the judge — for example *"How original is the concept?"*.
   There are no right or wrong answers; judges award points.
3. Repeats until every criterion has the questions they want.

Example setup:

| Criterion | Question |
|---|---|
| Innovation | How original is the concept? |
| Innovation | Does the team clearly state the problem? |
| Technical Execution | Does the prototype actually work? |
| Impact | Who benefits, and how big is the impact? |

---

## 4. How a judge scores a team

For each team, a judge:

1. Opens the team and reviews the project.
2. Goes through the criterion's questions one by one.
3. Gives each question a score, from **0 up to that question's maximum**.
4. Can leave a short **comment** for extra context.

### Extra questions ("other")

Sometimes a team does something worth noting that no catalogue question covers.
A judge can add an **ad-hoc question** on the spot: give it a name (optional), a
maximum score, and a score. It counts toward the result just like a normal
question.

### Skipping a question

If a question simply does not apply to a team, the judge can **skip** it. A
skipped question counts **neither for nor against** the team — it is removed from
both the points they earned and the points that were possible. This keeps teams
from being penalized for something that was never relevant to them.

---

## 5. Publishing results

Judging happens quietly first. Each judge's scores stay hidden until **that
judge publishes** the criterion. Once a judge publishes, their scores appear
on the live scoreboard and are combined with every other judge who has
published.

- **Each judge publishes independently.** You don't need to wait for the whole
  panel to publish a criterion for *your* scores to show up.
- **Averages across published judges.** If two judges publish, the board shows
  the average of their two scores for each team (not the sum). This keeps
  results fair even if judges scored differently.
- **Only published judges count.** An unpublished judge's scores are hidden
  until they publish. The board shows how many judges contributed (`judgesCounted`)
  so you can see if a team has been scored by the full panel or only part of it.
- **Only Admins can force-publish.** If needed, an Admin can publish all judges
  who have scores for a criterion (or a specific judge). Normal judges can only
  publish their own scores.
- **Publishing can be undone** if a mistake is found.
- **Re-submitting replaces your old score.** If you tweak a score after already
  submitting, resubmitting updates it (no duplicates).

---

## 6. How the scoreboard is ranked

The scoreboard is ranked by **percentage**, not by raw points. This makes it
fair even when challenges or criteria have different total point values.

For each team:

```
percentage = (points earned across published criteria)
             ÷ (total points possible across those criteria)
             × 100
```

Example:

| Criteria scored | Possible | Earned | Percentage |
|---|---|---|---|
| Innovation | 10 | 9 | — |
| **Total** | **10** | **9** | **90%** |

A team with 90% ranks above a team with 85%, regardless of how many total points
each challenge was worth.

The public board shows only:

- each team's overall percentage,
- each team's total earned and total possible points,
- the per-criterion totals (earned / possible).

It **never** shows the individual question scores or the comments — those stay
private to the judging panel and organizers.

---

## 7. Frequently asked questions

**Can two teams tie?**
Yes. Ties are shown clearly on the board; organizers decide how to break them.

**What if a judge forgets a question?**
The question is simply not counted until it is scored. Organizers can review all
scores before publishing.

**Do skipped questions lower a team's percentage?**
No. Skipped questions are removed from both sides of the calculation, so they
have no effect.

**Can we change the criteria after teams have been scored?**
Criteria can be edited, but the safest approach is to finalize the form before
judging begins. Organizers should review the form in advance.

**When do teams see the results?**
Only when a criterion is published. Until then, the board shows nothing for that
criterion.

**Who can see the detailed per-question scores?**
Only organizers and judges. The public sees percentages and totals only.
