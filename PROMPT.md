# Think — Engineer Scenario Simulator
# Claude Code Implementation Prompt

Build a complete ASP.NET Core 8 MVC web application called "Think — Engineer Scenario Simulator" to be hosted at think.thetruecode.com

---

## Overview

A platform where senior software engineers practice real-world decision making through simulated scenarios. Users land on a professional landing page, select a scenario, answer 7-8 questions in wizard style one at a time, and receive a final score with detailed feedback.

---

## Technical Requirements

- ASP.NET Core 8 MVC
- No database for V1 — all data stored as JSON files
- ASP.NET Core Session for tracking progress
- Bootstrap 5 for responsive layout
- No authentication
- Adding a new scenario must require only dropping a new JSON file — zero code changes

---

## Folder Structure

```
/Controllers
    HomeController.cs
    ScenarioController.cs
/Models
    ScenarioSummary.cs
    FullScenario.cs
    ScenarioStep.cs
    ScenarioOption.cs
    UserAnswer.cs
    AnswerReview.cs
    SessionResult.cs
/Data
    /scenarios
        payments-3am.json
        friday-deployment.json
        ceo-update.json
/Services
    ScenarioService.cs
/Views
    /Home
        Index.cshtml
    /Scenario
        Step.cshtml
        Result.cshtml
    /Shared
        _Layout.cshtml
/wwwroot
    /css
        site.css
    /js
        scenario.js
```

---

## Models

### ScenarioSummary
```csharp
string Id
string Title
string Description
string Type
string DifficultyLabel
int EstimatedMinutes
int TotalQuestions
bool IsAvailable
```

### ScenarioOption
```csharp
string Key        // A, B, C, D
string Text
```

### ScenarioStep
```csharp
int StepNumber
string Situation
List<string> Logs   // format: "ERROR|message" or "WARN|message" or "INFO|message"
string Question
List<ScenarioOption> Options
int CorrectIndex
string Explanation
string SeniorSays
```

### FullScenario
```csharp
string Id
string Title
string Type
string Difficulty
List<ScenarioStep> Steps
```

### UserAnswer
```csharp
string ScenarioId
int StepNumber
int SelectedIndex
bool IsCorrect
```

### AnswerReview
```csharp
int StepNumber
string Question
string SelectedOptionText
string CorrectOptionText
bool IsCorrect
string Explanation
string SeniorSays
```

### SessionResult
```csharp
string ScenarioId
string ScenarioTitle
int TotalQuestions
int CorrectAnswers
int ScorePercent
string ScoreTitle
string ScoreDescription
List<AnswerReview> Answers
```

---

## ScenarioService

Register as Singleton in Program.cs.

- On startup, load and cache all JSON files from folder defined in appsettings.json key "ScenariosPath"
- `List<ScenarioSummary> GetAllSummaries()` — returns list sorted: available first, then coming soon
- `FullScenario GetById(string id)` — returns full scenario with all steps
- New JSON file added to folder plus app restart equals automatically picked up, no code changes needed

---

## Controllers

### HomeController
`Index()` — loads all summaries via ScenarioService, passes to view

### ScenarioController

`Start(string id)` GET
- Load FullScenario from ScenarioService
- Store in Session as serialized JSON
- Clear any previous answers for this scenario from Session
- Redirect to Step(id, 1)

`Step(string id, int stepNumber)` GET
- Load scenario from Session
- Get step matching stepNumber
- If stepNumber out of range redirect to Result(id)
- Pass to view: current step, stepNumber, totalSteps, scenarioTitle, scenarioType

`Submit(string id, int stepNumber, int selectedIndex)` POST — AJAX only
- Load scenario from Session
- Validate selectedIndex against CorrectIndex
- Save UserAnswer to Session list
- Return JSON response:
```json
{
  "isCorrect": true,
  "correctIndex": 1,
  "explanation": "...",
  "seniorSays": "...",
  "nextStepNumber": 2,
  "isLastStep": false
}
```

`Result(string id)` GET
- Load UserAnswers and FullScenario from Session
- Calculate score and build AnswerReview list
- Generate ScoreTitle:
  - 100% → "Staff Engineer Instincts"
  - 75% and above → "Senior Instincts"
  - 50% and above → "Mid-Level Thinking"
  - Below 50% → "Still Learning"
- Generate ScoreDescription matching the ScoreTitle
- Pass SessionResult to view

---

## Session Keys

- `"scenario_{id}_data"` — FullScenario serialized as JSON string
- `"scenario_{id}_answers"` — List of UserAnswer serialized as JSON string

---

## Views

### _Layout.cshtml
- Top nav: app name "think.thetruecode.com" on left, Home link on right
- Footer: "by Gaurav Sharma" with link to https://www.thetruecode.com
- Bootstrap 5 CDN
- Google Fonts: Syne for headings, Lora for body, JetBrains Mono for logs
- Link site.css and scenario.js

---

### Home/Index.cshtml — Landing Page

Must be professional, polished and clearly explain the platform to a first-time visitor.

**Section 1 — Hero**
- Bold headline: "Can you handle production reality?"
- Subheadline: "Real-world scenarios for engineers who want to sharpen their judgment under pressure. No syntax tests. No theory. Just decisions that matter."
- How It Works block with 3 steps:
  1. Choose a scenario from real engineering situations
  2. Answer 7-8 questions one at a time in wizard style
  3. Get your score and learn what a senior engineer would do
- CTA button that scrolls to scenario list

**Section 2 — About**
- Short paragraph: This platform simulates the moments that define engineering careers — production incidents, deployment decisions, stakeholder communication under pressure. Each scenario is based on real patterns from 20+ years of engineering experience.

**Section 3 — Scenario Cards**
- Heading: "Choose Your Scenario"
- Responsive grid: 3 columns desktop, 2 tablet, 1 mobile
- Each available card shows: type badge, title, description, difficulty, estimated time, question count, Start Scenario button linking to ScenarioController/Start/{id}
- Unavailable cards: same info but Coming Soon badge instead of button, card visually muted

---

### Scenario/Step.cshtml — Question Wizard

**Progress bar — top of page, always visible**
- Full width bar filling proportionally: stepNumber divided by totalSteps
- Label: "Question {stepNumber} of {totalSteps}"
- First element the user sees on every question page

**Scenario context**
- Scenario type and title
- Situation text
- If logs exist: monospace code block with lines colored by prefix
  - ERROR → red
  - WARN → amber
  - INFO → green

**Question**
- Question text
- Four option buttons A B C D with full descriptive text
- On click: POST to Submit via JavaScript fetch
- Disable all options immediately after selection

**After AJAX response — no page reload**
- Correct: green border and background on selected option
- Wrong: red on selected, green on correct option
- Show explanation text
- Show SeniorSays as styled quote block
- Show Next button: links to next step or Result page if last step

---

### Scenario/Result.cshtml — Score Page

**Score summary**
- Large: "{CorrectAnswers} out of {TotalQuestions}"
- ScoreTitle as heading
- ScoreDescription as paragraph

**Full question review**
For each question:
- Question text
- User answer with green tick or red cross
- If wrong: correct answer shown
- Explanation
- SeniorSays as styled quote

**Buttons**
- Try Another Scenario → Home/Index
- Share on LinkedIn → LinkedIn share URL with score and site URL pre-filled
- Read the Blog → https://www.thetruecode.com

---

## JavaScript — scenario.js

- Handle option button click on Step view
- POST to Submit via fetch with scenarioId, stepNumber, selectedIndex
- On response: update styles, show explanation and SeniorSays, show Next button
- Disable all options after first click
- Animate progress bar width on page load

---

## JSON Scenario File Format

Every file in /Data/scenarios/ follows this exact structure.
App reads all files automatically — no registration needed.

```json
{
  "id": "unique-kebab-case-id",
  "title": "Scenario title",
  "description": "Short description for card",
  "type": "Production Incident",
  "difficulty": "Hard",
  "estimatedMinutes": 12,
  "isAvailable": true,
  "steps": [
    {
      "stepNumber": 1,
      "situation": "Scene setting paragraph for this question.",
      "logs": [
        "ERROR|timestamp message",
        "WARN|timestamp message",
        "INFO|timestamp message"
      ],
      "question": "What do you do?",
      "options": [
        { "key": "A", "text": "Full descriptive text of option A" },
        { "key": "B", "text": "Full descriptive text of option B" },
        { "key": "C", "text": "Full descriptive text of option C" },
        { "key": "D", "text": "Full descriptive text of option D" }
      ],
      "correctIndex": 1,
      "explanation": "Why this answer is correct with full reasoning.",
      "seniorSays": "What a senior engineer would say about this decision."
    }
  ]
}
```

---

## Sample Data — Create These Three Files

### payments-3am.json
```json
{
  "id": "payments-3am",
  "title": "3am. Payments down. 6 engineers on a call.",
  "description": "A payment service starts failing 40% of requests at 3am. Logs are noisy. No obvious cause. You are leading the bridge call.",
  "type": "Production Incident",
  "difficulty": "Hard",
  "estimatedMinutes": 12,
  "isAvailable": true,
  "steps": [
    {
      "stepNumber": 1,
      "situation": "It is 3:17am. PagerDuty fires for the payments service. You are on call. Monitoring shows 40% of POST /payments/process requests returning 500 errors. The other 60% succeed. Slack is active with a client-facing team asking for updates. Three engineers join the bridge call in 2 minutes.",
      "logs": [
        "ERROR|03:14:52 PaymentProcessor: NullReferenceException at OrderService.cs:247",
        "WARN|03:14:52 DB connection pool: 89/100 connections active",
        "INFO|03:14:53 Retry succeeded for txn_8a2f1c",
        "ERROR|03:14:54 PaymentProcessor: NullReferenceException at OrderService.cs:247",
        "WARN|03:14:55 Circuit breaker: HALF_OPEN state"
      ],
      "question": "The bridge call starts. One engineer says roll back the last deployment. Another says check the database first. You are leading. What is your first action?",
      "options": [
        { "key": "A", "text": "Immediately trigger a rollback. A recent deployment is the most likely cause and rollback is the fastest path to resolution." },
        { "key": "B", "text": "Assign roles on the call — one person on logs, one on database metrics, one handling Slack updates — then begin parallel investigation." },
        { "key": "C", "text": "Ask everyone to go quiet while you personally investigate logs and metrics for five minutes before deciding anything." },
        { "key": "D", "text": "Escalate immediately to your engineering manager. A production incident at this hour needs senior sign-off before any action." }
      ],
      "correctIndex": 1,
      "explanation": "Parallel investigation with clear role assignment is the right move. Rolling back without evidence is risky — you might revert a fine deployment while the real cause continues. Chaos on a bridge call without structure wastes critical minutes.",
      "seniorSays": "A senior's first job on a bridge call is structure, not solutions. Assign roles, investigate in parallel, then act on evidence."
    },
    {
      "stepNumber": 2,
      "situation": "Five minutes in. DB connections are at 92 of 100. A junior engineer spots a deployment that went out two hours ago. The NullReferenceException is always on the same line — OrderService.cs:247. A Slack message arrives from a client-facing manager asking for an ETA.",
      "logs": [
        "WARN|03:19:01 DB connection pool: 92/100 connections active",
        "ERROR|03:19:03 PaymentProcessor: NullReferenceException at OrderService.cs:247",
        "INFO|03:19:04 txn_9c3d2e: succeeded",
        "WARN|03:19:05 DB connection pool: 94/100 — approaching limit"
      ],
      "question": "The Slack message says: Can you give us an ETA? You do not know the root cause yet. How do you respond?",
      "options": [
        { "key": "A", "text": "Ignore Slack for now. Your full attention must stay on debugging. Someone else can handle communication later." },
        { "key": "B", "text": "Send a short message: We are actively investigating. I will give a full update in 15 minutes. Short, honest, time-boxed." },
        { "key": "C", "text": "Send a confident estimate: Should be resolved in 30 minutes. It manages expectations and keeps stakeholders calm." },
        { "key": "D", "text": "Share the full technical picture in Slack — DB pool at 94%, NullRef on line 247, deployment two hours ago — so they understand the complexity." }
      ],
      "correctIndex": 1,
      "explanation": "Never give a confident ETA without knowing root cause. Ignoring Slack creates panic and escalations. Dumping raw technical details on non-technical stakeholders adds confusion. A short honest time-boxed update is always the professional move.",
      "seniorSays": "Communication runs in parallel to debugging, not after it. I do not know yet but will update in 15 minutes is a complete and professional answer."
    },
    {
      "stepNumber": 3,
      "situation": "The engineer on logs finds the cause. A background job called ReportExport was added in the recent deployment. It runs a database query every 30 seconds but never disposes the connection. Classic connection leak. DB pool is now at 98 of 100. Payments will fail completely in minutes.",
      "logs": [
        "ERROR|03:22:11 DB connection pool: 98/100 — CRITICAL",
        "ERROR|03:22:12 PaymentProcessor: Timeout waiting for DB connection (5000ms exceeded)",
        "WARN|03:22:12 BackgroundJobRunner: Job ReportExport running for 18 minutes",
        "ERROR|03:22:13 PaymentProcessor: NullReferenceException at OrderService.cs:247"
      ],
      "question": "Root cause found — ReportExport job is leaking DB connections. Pool at 98 of 100. What do you do right now?",
      "options": [
        { "key": "A", "text": "Kill the ReportExport job process immediately. Stop the leak now, fix the code properly afterward." },
        { "key": "B", "text": "Roll back the entire deployment that introduced the job even if it contains other unrelated changes." },
        { "key": "C", "text": "Increase the DB connection pool limit as a temporary measure while the team prepares a proper fix." },
        { "key": "D", "text": "Fix the connection disposal bug in code right now, test it, and deploy a hotfix." }
      ],
      "correctIndex": 0,
      "explanation": "Stop the bleed first. Killing the leaking job is the fastest and most surgical fix. Full rollback risks reverting good changes. Increasing pool size delays the inevitable crash. A hotfix at 3am under pressure with no proper review risks creating incident number two.",
      "seniorSays": "In production: surgical over nuclear. Kill the one bad process. Fix the code tomorrow when you are rested and the system is stable."
    },
    {
      "stepNumber": 4,
      "situation": "Job killed. DB connections drop to 31 of 100 within 90 seconds. Payment success rate back to 99.8%. Incident resolved. It is 3:31am. The team is still on the call.",
      "logs": [
        "INFO|03:31:04 DB connection pool: 31/100 — normal",
        "INFO|03:31:05 PaymentProcessor: txn_a1b2c3 succeeded",
        "INFO|03:31:06 PaymentProcessor: txn_d4e5f6 succeeded",
        "INFO|03:31:07 Circuit breaker: CLOSED — service healthy"
      ],
      "question": "Incident resolved. It is 3:31am. What do you do next?",
      "options": [
        { "key": "A", "text": "Close the call and go back to sleep. The incident is over. Post-mortem and documentation can wait until morning." },
        { "key": "B", "text": "Send an all-clear on Slack, write quick raw notes while it is fresh, then rest. Schedule post-mortem for morning." },
        { "key": "C", "text": "Start writing the full post-mortem right now. Memory is freshest in this moment and it will save time tomorrow." },
        { "key": "D", "text": "Deploy the hotfix for the connection leak immediately while the team is still assembled on the call." }
      ],
      "correctIndex": 1,
      "explanation": "Send the all-clear so stakeholders know it is resolved. Capture raw notes — not a full post-mortem, just bullet points. Then rest. Writing a full post-mortem at 3:31am produces poor quality work. Deploying a hotfix under fatigue risks creating a new incident.",
      "seniorSays": "Post-incident: communicate closure, capture context, close your laptop. A tired engineer writing docs or deploying at 3am is a liability."
    },
    {
      "stepNumber": 5,
      "situation": "Next morning. You are writing the post-mortem. Your manager asks you to include root cause, timeline, and prevention. A junior engineer suggests the prevention section should say: remind developers to always dispose connections properly.",
      "logs": [],
      "question": "The junior suggests prevention equals reminding developers to dispose connections. What is your response?",
      "options": [
        { "key": "A", "text": "Agree. Add it to the post-mortem. Developer awareness is practical and the fastest fix." },
        { "key": "B", "text": "Disagree. Prevention must be systemic — add automated connection leak detection in CI pipeline and alerts when pool crosses 80%. Do not rely on human memory." },
        { "key": "C", "text": "Partially agree. Add both — the reminder and a code review checklist item for connection disposal." },
        { "key": "D", "text": "Escalate to the architect to decide the prevention strategy. This decision is above your level." }
      ],
      "correctIndex": 1,
      "explanation": "Good post-mortems never rely on people trying harder as prevention. Humans make mistakes under pressure — systems need safeguards. Automated detection in CI and alerting before the pool is critical are systemic fixes that will actually prevent recurrence.",
      "seniorSays": "If your prevention section says tell people to be more careful, your post-mortem is not done. Make the system catch it, not the human."
    },
    {
      "stepNumber": 6,
      "situation": "Two days later. A different team asks if the ReportExport job can be re-enabled. Their weekly report has not run. The fix is ready and code reviewed. But it is Thursday afternoon and another deployment is planned for tomorrow.",
      "logs": [],
      "question": "The fixed ReportExport job is ready. Another deployment is planned for tomorrow. When do you re-enable the job?",
      "options": [
        { "key": "A", "text": "Re-enable it today as an isolated change. The fix is small, reviewed, and the business needs the report. No reason to wait." },
        { "key": "B", "text": "Bundle it with tomorrow's deployment. Fewer deployment events means fewer chances for something to go wrong." },
        { "key": "C", "text": "Wait until next week. The team just had an incident and needs breathing room before more changes." },
        { "key": "D", "text": "Re-enable in staging only today. Keep production disabled until Monday when full team support is available." }
      ],
      "correctIndex": 0,
      "explanation": "Small isolated and well-reviewed fixes should be deployed independently and promptly. Bundling them with unrelated changes makes root cause analysis harder if something goes wrong. The fix is ready, the business impact is real. Deploy it cleanly today with monitoring.",
      "seniorSays": "Deploy small changes independently. Bundling a fix with unrelated work is how a clean change becomes part of a messy incident report."
    },
    {
      "stepNumber": 7,
      "situation": "A week has passed. Your manager asks you to present the incident to the wider engineering team as a learning session. A colleague says: why would we embarrass the team that introduced the bug by making this public?",
      "logs": [],
      "question": "Your colleague questions whether the public incident review will embarrass the team responsible. How do you respond?",
      "options": [
        { "key": "A", "text": "Agree with the colleague. Keep the incident review internal to the immediate team only. No need to share it publicly." },
        { "key": "B", "text": "Explain that blameless post-mortems share learnings not assign fault. Present systemic failures and fixes, not individual mistakes. The whole engineering org benefits." },
        { "key": "C", "text": "Present it publicly but remove all references to which team or deployment caused it. Anonymize completely." },
        { "key": "D", "text": "Let the team that caused the incident decide whether they are comfortable with a public review." }
      ],
      "correctIndex": 1,
      "explanation": "Blameless post-mortems are a cornerstone of high-performing engineering cultures. The goal is systemic learning not individual accountability. If teams fear sharing incidents, problems get hidden and the whole organization learns nothing. A well-run review focuses on what failed in the system, not who made a mistake.",
      "seniorSays": "The team that shares their incident openly earns the most respect. Hiding failures is how organizations repeat them."
    }
  ]
}
```

### friday-deployment.json
```json
{
  "id": "friday-deployment",
  "title": "Friday 4pm. Release approved. Do you deploy?",
  "description": "Feature is tested and stakeholders are waiting. The team leaves for the weekend in an hour. Classic dilemma every engineer faces.",
  "type": "Deployment Decision",
  "difficulty": "Medium",
  "estimatedMinutes": 10,
  "isAvailable": false,
  "steps": []
}
```

### ceo-update.json
```json
{
  "id": "ceo-update",
  "title": "System is down. CEO wants an update in 10 minutes.",
  "description": "Root cause is unknown. Team is still investigating. How do you communicate what you do not yet know to the most senior person in the company?",
  "type": "Stakeholder Communication",
  "difficulty": "Senior",
  "estimatedMinutes": 8,
  "isAvailable": false,
  "steps": []
}
```

---

## appsettings.json

Add this key:
```json
"ScenariosPath": "Data/scenarios"
```

---

## Program.cs

- Register ScenarioService as Singleton
- Enable Session middleware with 60 minute idle timeout
- Configure routing for HomeController and ScenarioController
- Use System.Text.Json for all serialization

---

## README.md

Include:
- What this application is
- How to run locally
- How to add a new scenario: create a JSON file in Data/scenarios following the schema above, restart the app, it appears automatically with no code changes
- How to deploy to Azure App Service
