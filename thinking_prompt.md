Build a standalone web application called "thinking" — an architecture
interview simulation platform for senior engineers. Think of it as a
flight simulator for system design interviews. 

REFERENCE INSPIRATION (do not copy code, just understand the UX pattern):
think.thetruecode.com — a wizard-style scenario simulation platform.
Same progressive step-by-step feel, but adapted for interview simulation.

---

TECH STACK:
- ASP.NET Core MVC (.NET 8)
- C# 
- Razor Views (.cshtml)
- Vanilla JavaScript (no frameworks — keep it simple and fast)
- CSS (single custom stylesheet — no Tailwind, no Bootstrap)
- JSON files as the ONLY data source (no database, no EF Core)
- Newtonsoft.Json or System.Text.Json for deserializing scenario JSON
- No npm, no node_modules, no build pipeline

---

PROJECT STRUCTURE:
/Controllers
  HomeController.cs          (scenario listing)
  ScenarioController.cs      (scenario player, step navigation, scoring)

/Models
  Scenario.cs
  Step.cs
  AnswerOption.cs
  IdealAnswer.cs
  IdealAnswerSection.cs
  ScoreCard.cs
  ScoreBand.cs
  SessionResult.cs           (tracks user answers + scores in TempData)

/Views
  /Home
    Index.cshtml             (scenario listing grid)
  /Scenario
    Player.cshtml            (main simulation screen)
    Score.cshtml             (final scorecard screen)
  /Shared
    _Layout.cshtml
    _InterviewerPanel.cshtml (left panel partial)
    _IdealAnswerPanel.cshtml (right panel partial)

/Data
  /Scenarios
    gdpr-multi-tenancy.json
    (more scenario JSON files here)

/Services
  ScenarioLoader.cs          (reads and deserializes JSON from /Data)
  ScoreCalculator.cs         (computes score from session answers)

/wwwroot
  /css
    site.css                 (single master stylesheet)
  /js
    scenario-player.js       (answer selection, panel reveal, animations)
    scorecard.js             (score count-up animation)

---

JSON SCHEMA (single source of truth — build all models around this):

{
  "id": "gdpr-multi-tenancy",
  "title": "GDPR & Multi-Tenant Architecture",
  "category": "Multi-tenancy Design",
  "difficulty": "Senior",
  "estimatedMinutes": 15,
  "description": "A German enterprise client is evaluating your LMS 
                   platform. Navigate the architecture interview.",
  "tags": ["multi-tenancy", "GDPR", "Azure", "SQL Server", "Redis"],

  "interviewer": {
    "name": "Sarah Chen",
    "role": "Principal Architect, FAANG",
    "avatar": "female-architect"
  },

  "steps": [
    {
      "stepNumber": 1,
      "interviewerSays": "A German enterprise client asks: How is their
                          data isolated from other tenants on your platform?",
      "context": "Opening question. They want to see if you understand
                  multi-tenancy models at depth.",
      "type": "single-choice",
      "options": [
        {
          "id": "A",
          "text": "Shared database with TenantId column on every table",
          "score": 3,
          "tag": "Partially Correct",
          "tagColor": "yellow",
          "interviewerReaction": "Hmm — what about GDPR compliance 
                                  for this specific client?"
        },
        {
          "id": "B",
          "text": "Dedicated Geo-Pod in Germany with isolated compute,
                   database, cache, storage and logging",
          "score": 10,
          "tag": "Excellent",
          "tagColor": "green",
          "interviewerReaction": "Good thinking. Now how do you handle
                                  tenant suspension for this client?"
        },
        {
          "id": "C",
          "text": "Separate database per tenant globally",
          "score": 5,
          "tag": "Incomplete",
          "tagColor": "orange",
          "interviewerReaction": "You addressed isolation but missed
                                  the data residency requirement."
        }
      ],
      "idealAnswer": {
        "summary": "For a GDPR-regulated German client, the correct
                    approach is a full regional Geo-Pod deployment.",
        "sections": [
          {
            "type": "text",
            "heading": "The Three Isolation Models",
            "content": "Multi-tenancy has three primary models: Pool 
                        (shared DB + TenantId), Bridge (shared DB + 
                        separate schema), and Silo (dedicated DB per 
                        tenant). For GDPR clients, Silo is mandatory 
                        regardless of their plan tier."
          },
          {
            "type": "architecture-block",
            "heading": "EU Geo-Pod Architecture",
            "content": "┌─────────────────────────────────────┐\n│        EU GEO-POD (Germany)         │\n│                                     │\n│  ┌────────┐ ┌───────┐ ┌─────────┐  │\n│  │  AKS   │ │ Redis │ │  SQL DB │  │\n│  │ Nodes  │ │ Cache │ │ Germany │  │\n│  └────────┘ └───────┘ └─────────┘  │\n│  ┌──────────────────────────────┐   │\n│  │   Blob Storage (Germany)     │   │\n│  └──────────────────────────────┘   │\n│  ┌──────────────────────────────┐   │\n│  │  Log Analytics (EU only)     │   │\n│  └──────────────────────────────┘   │\n└─────────────────────────────────────┘"
          },
          {
            "type": "key-principles",
            "heading": "Key Principles",
            "items": [
              "GDPR compliance overrides cost optimization decisions",
              "ALL layers must be regional — compute, cache, DB, storage and logs",
              "Backups replicate only to paired EU region (germanynorth)",
              "Log Analytics is the layer teams most commonly forget"
            ]
          },
          {
            "type": "warning",
            "heading": "Most Common Production Mistake",
            "content": "Teams correctly isolate the SQL database but 
                        forget that Redis cache and Log Analytics 
                        workspaces also contain PII — user sessions, 
                        activity logs, IP addresses. All of these 
                        must stay within the German region boundary."
          },
          {
            "type": "tradeoffs",
            "heading": "Trade-offs",
            "pros": [
              "Full GDPR compliance and legal defensibility",
              "Strong tenant isolation at every layer",
              "Independent backup and restore per tenant"
            ],
            "cons": [
              "Higher infrastructure cost — pass as Data Residency Premium tier",
              "More complex automated onboarding pipeline",
              "Operational overhead of managing regional stacks"
            ]
          }
        ],
        "oneLineSummary": "GDPR compliance is not just a database 
                           decision — it is a full-stack regional 
                           architecture decision."
      }
    }
  ],

  "scoreCard": {
    "maxScore": 30,
    "bands": [
      {
        "min": 27,
        "max": 30,
        "label": "Ready for this SA role",
        "color": "green",
        "message": "Strong architectural thinking across all dimensions.
                    You demonstrated both breadth and depth."
      },
      {
        "min": 18,
        "max": 26,
        "label": "Strong but gaps exist",
        "color": "yellow",
        "message": "Good fundamentals. Deepen your understanding of 
                    compliance architecture and distributed systems."
      },
      {
        "min": 0,
        "max": 17,
        "label": "Needs deeper preparation",
        "color": "red",
        "message": "Revisit multi-tenancy fundamentals and GDPR 
                    architecture requirements before your interview."
      }
    ]
  }
}

---

ROUTING & CONTROLLER LOGIC:

HomeController:
  GET /                        → Load all JSON files from /Data/Scenarios
                                 Deserialize to List<ScenarioSummary>
                                 Pass to Index.cshtml

ScenarioController:
  GET  /scenario/{id}          → Load scenario JSON by id
                                 Initialize session state in TempData
                                 Show Step 1 → Player.cshtml

  POST /scenario/{id}/answer   → Receive: stepNumber, selectedOptionId
                                 Store answer + score in TempData
                                 Return JSON response:
                                 {
                                   "score": 10,
                                   "tag": "Excellent",
                                   "tagColor": "green",
                                   "interviewerReaction": "...",
                                   "idealAnswer": { ...full object... },
                                   "isLastStep": false,
                                   "nextStepNumber": 2
                                 }

  GET  /scenario/{id}/step/{n} → Load scenario, return step n view
  
  GET  /scenario/{id}/score    → Read all answers from TempData
                                 Calculate total score
                                 Match to score band
                                 Show Score.cshtml

TempData keys:
  "answers_{scenarioId}"       → JSON array of 
                                 {stepNumber, optionId, score}

---

UI/UX REQUIREMENTS — THIS IS THE MOST CRITICAL PART:

1. OVERALL FEEL:
   - Dark theme — deep navy background (#0f1629)
   - Panel backgrounds: slightly lighter (#1a2340)
   - Accent color: cyan (#00d4ff) for highlights and CTAs
   - Warning color: amber (#f59e0b)
   - Success color: emerald (#10b981)
   - Premium and serious — like a cockpit instrument panel
     meets a technical interview room
   - No gradients that look cheap — subtle, purposeful only
   - Clean monospace font for code/diagrams (Fira Code or 
     JetBrains Mono via Google Fonts)
   - Body font: Inter (Google Fonts)

2. HOME PAGE (Index.cshtml):
   - Full dark page, centered header:
     "ArchSim" in large bold white
     Subtitle: "Architecture Interview Simulator for Senior Engineers"
   - Scenario cards in responsive CSS grid (3 cols desktop, 
     2 tablet, 1 mobile)
   - Each card:
     → Title (white, bold)
     → Category (cyan, small caps)
     → Difficulty badge (color coded):
        Junior=blue, Mid=yellow, Senior=orange, Principal=red
     → Estimated time + tag pills
     → Description (slate-400 color)
     → "Start Simulation →" CTA button (full width, cyan)
   - Card hover: translateY(-4px) + box-shadow lift
   - Card border: 1px solid rgba(255,255,255,0.08)
   - Smooth transition on hover (0.2s ease)

3. SCENARIO PLAYER (Player.cshtml) — TWO PANEL LAYOUT:

   LEFT PANEL (38% width, fixed while right scrolls):
   
   - TOP: Progress bar
     → "Step 2 of 5" label
     → Thin progress bar, cyan fill, smooth width transition
   
   - INTERVIEWER BLOCK:
     → Avatar: CSS-generated geometric avatar 
       (initials in colored circle — no image files needed)
     → Name + role in smaller muted text
     → Speech bubble containing interviewerSays text
       → Slight left border in cyan
       → Italic, larger font, white
       → Feels like someone is actually talking to you
     → Context hint below bubble:
       Smaller, slate-500, italic
       "💡 " + context text
   
   - ANSWER OPTIONS (below interviewer block):
     → Each option is a clickable card
     → Letter badge on left (A / B / C) in muted circle
     → Option text on right
     → Full card is clickable (not just text)
     → Hover: border color shifts to cyan, 
               letter badge fills cyan
     → On click:
         → Selected card: border cyan, slight bg highlight
         → All other cards: dim to 40% opacity, not clickable
         → Score tag badge animates in on selected card:
           (Excellent=green, Partially Correct=yellow, 
            Incomplete=orange)
         → Interviewer reaction text fades in below options
         → POST to /scenario/{id}/answer via fetch()
         → Right panel reveals (see below)
   
   RIGHT PANEL (62% width, scrollable):
   
   - DEFAULT STATE (before answer selected):
     → Centered message, muted:
       Lock icon + "Select an answer to reveal 
       the ideal explanation"
     → Subtle dashed border around empty panel
   
   - AFTER ANSWER SELECTED (panel animates open):
     → Slide in from right + fade (CSS transition)
     → Renders IdealAnswer sections sequentially
       Each section fades up with 120ms stagger delay
     
     SECTION RENDERING BY TYPE:
     
     "text"
     → Heading: white, bold, border-bottom cyan 1px, mb-3
     → Content: slate-300, line-height 1.8, readable prose
     
     "architecture-block"  
     → Heading same as above
     → Content in <pre> block:
        background: #0a0f1e (darker than page)
        border: 1px solid rgba(0,212,255,0.2)
        border-left: 3px solid cyan
        font-family: monospace
        color: #a8d8ea
        padding: 1.5rem
        overflow-x: auto
        border-radius: 6px
        Renders ASCII art diagrams perfectly
     
     "key-principles"
     → Heading same as above
     → Numbered list:
        Number: cyan, bold, monospace
        Text: slate-200
        Each item has subtle bottom border
        Spacing generous
     
     "warning"
     → Entire block: amber/yellow tinted bg 
       rgba(245,158,11,0.08)
       border-left: 3px solid #f59e0b
       border-radius: 6px
       padding: 1rem 1.5rem
     → Heading: amber color + ⚠ icon
     → Content: slate-300
     
     "tradeoffs"
     → Heading same as above
     → Two column layout side by side:
        LEFT: "✓ Advantages" header in emerald
              Each pro: ✓ icon in emerald + text
        RIGHT: "✗ Trade-offs" header in red
               Each con: ✗ icon in red + text
        Columns separated by subtle vertical divider
     
     → BOTTOM OF PANEL (after all sections):
        Divider line
        One-line summary in larger italic cyan text
        Feels like the "punchline" of the explanation
        
     → "Next Question →" button:
        Appears last, full width
        Cyan background, dark text, bold
        On click: fetch next step or redirect to score

4. SCORE CARD (Score.cshtml):
   - Full centered dark page
   - Large heading: "Simulation Complete"
   - Circular score display:
     → SVG circle progress indicator
     → Center: "24 / 30" in large white bold
     → Circle stroke: cyan, animated on load
   - Score band label below circle:
     → Large text in band color
     → e.g. "Strong but gaps exist"
   - Band message in slate-300 below
   - Per-step breakdown table:
     → Columns: Step | Question (truncated) | 
                Your Answer | Score | Max | Result
     → Alternating row backgrounds (subtle)
     → Result column: colored tag badges
   - Two CTA buttons side by side:
     → "Try Again" (outlined, cyan border)
     → "← Back to Scenarios" (filled cyan)

5. ANIMATIONS (pure CSS + vanilla JS — no libraries):
   - Answer cards stagger in on page load:
     opacity 0 → 1, translateY(10px) → 0
     50ms delay between each card
   - Selected answer: brief scale(1.02) pulse then back
   - Right panel reveal: 
     transform: translateX(20px) → translateX(0)
     opacity: 0 → 1
     transition: 0.35s ease
   - Section stagger: each section delays 120ms more than previous
     opacity: 0 → 1, translateY(8px) → 0
   - Score circle: SVG stroke-dashoffset animates from 0 
     to final value on page load (0.8s ease)
   - Score number: JS count-up from 0 to final score (600ms)

6. RESPONSIVE BREAKPOINTS:
   - > 1024px: two panel side by side
   - 768-1024px: stacked, left panel full width, 
                 right panel below (no height restriction)
   - < 768px: fully stacked, simplified, 
              all animations reduced

---

SESSION STATE STRATEGY:
- Use TempData (backed by Session) to track:
  → Current scenario id
  → Array of {stepNumber, selectedOptionId, score}
- On each answer POST, append to TempData array
- On score page, read full array and calculate
- Do NOT use hidden form fields for score tracking
  (user could manipulate them)
- Session timeout: 30 minutes (standard)
- Configure in Program.cs:
  builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
  });

---

SCENARIOLOADER SERVICE:
- Reads all .json files from /Data/Scenarios/ at startup
- Deserializes using System.Text.Json
- Caches in IMemoryCache (key: "scenarios")
- Exposes:
  List<ScenarioSummary> GetAll()
  Scenario GetById(string id)
- ScenarioSummary is a lightweight model for the home page
  (id, title, category, difficulty, estimatedMinutes, 
   description, tags — no steps loaded)

---

SAMPLE DATA:
Create one complete working scenario JSON file:
gdpr-multi-tenancy.json
Use the full JSON schema above with at least 3 complete steps.
Each step must have 3 options and a full idealAnswer with 
at least 4 sections of mixed types 
(text, architecture-block, key-principles, warning or tradeoffs).
Make the content architecturally accurate and genuinely useful 
to a senior engineer preparing for an SA interview.
The scenario should cover:
  Step 1: Multi-tenancy model selection for GDPR client
  Step 2: Tenant identification and routing strategy
  Step 3: Cache invalidation on tenant suspension

---

QUALITY BAR:
- The right panel ideal answer must feel like reading a 
  senior architect's detailed explanation — not a quiz answer
- ASCII diagrams must render cleanly in monospace blocks
- Typography must be readable — generous line-height, 
  good contrast, not cramped
- The product must feel premium — someone should look at 
  this and think "this is a serious tool"
- Zero external JS dependencies — everything in one 
  vanilla scenario-player.js file
- Zero npm — pure ASP.NET Core MVC project, 
  dotnet run and it works

---

START WITH:
1. Create the project structure
2. Define all C# models matching the JSON schema exactly
3. Build ScenarioLoader service
4. Create the one sample JSON scenario (3 steps, full content)
5. Build HomeController + Index view
6. Build ScenarioController + Player view (two panel layout)
7. Build Score view
8. Write site.css (complete, no missing styles)
9. Write scenario-player.js (answer selection, fetch, 
   panel reveal, animations)
10. Wire everything up, verify dotnet run works cleanly

Do not skip steps. Do not leave placeholder CSS. 
Do not leave TODO comments. 
Deliver a working, visually complete application.