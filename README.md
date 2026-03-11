# Think — Engineer Scenario Simulator

A platform where senior software engineers practice real-world decision making through simulated scenarios. Users land on a professional landing page, select a scenario, answer 7–8 questions in wizard style one at a time, and receive a final score with detailed feedback.

Hosted at: **think.thetruecode.com**

---

## What This Application Is

Think puts you in realistic engineering situations — production incidents, deployment decisions, stakeholder communication under pressure — and tests your judgment. No syntax. No theory. Just decisions that matter.

Each scenario walks you through 7–8 questions one at a time. After each answer you see whether you were right, a full explanation of why, and what a senior engineer would actually do. At the end you receive a score and a complete review of every question.

---

## How to Run Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)

### Steps

```bash
# Clone the repo
git clone <repo-url>
cd thinking1

# Run the application
dotnet run

# Open in browser
# https://localhost:5001  or  http://localhost:5000
```

The app loads scenario JSON files from `Data/scenarios/` automatically on startup.

---

## How to Add a New Scenario

1. Create a new `.json` file in `Data/scenarios/` following this schema:

```json
{
  "id": "unique-kebab-case-id",
  "title": "Scenario title",
  "description": "Short description shown on the card",
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

2. Restart the application.

The new scenario appears automatically on the home page. **Zero code changes required.**

- Set `"isAvailable": false` to show the scenario as "Coming Soon" (no start button, card is muted).
- The `logs` array can be empty (`[]`) for questions that don't involve log analysis.

---

## How to Deploy to Azure App Service

### Using Azure CLI

```bash
# Build and publish
dotnet publish -c Release -o ./publish

# Create resource group and App Service (if not existing)
az group create --name think-rg --location eastus
az appservice plan create --name think-plan --resource-group think-rg --sku B1 --is-linux
az webapp create --name think-thetruecode --resource-group think-rg --plan think-plan --runtime "DOTNET|8.0"

# Deploy
az webapp deploy --resource-group think-rg --name think-thetruecode --src-path ./publish --type zip
```

### Using Visual Studio Publish

1. Right-click the project → Publish
2. Choose Azure → Azure App Service (Linux)
3. Follow the wizard to create or select an existing App Service
4. Click Publish

### Custom Domain

In Azure Portal: App Service → Custom domains → Add `think.thetruecode.com` and configure your DNS records accordingly.

---

## Project Structure

```
/Controllers
    HomeController.cs       — Landing page
    ScenarioController.cs   — Start, Step, Submit, Result
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
        payments-3am.json   — Available scenario
        friday-deployment.json  — Coming soon
        ceo-update.json         — Coming soon
/Services
    ScenarioService.cs      — Loads and caches all JSON files on startup
/Views
    /Home
        Index.cshtml        — Landing page
    /Scenario
        Step.cshtml         — Question wizard
        Result.cshtml       — Score and review
    /Shared
        _Layout.cshtml
/wwwroot
    /css
        site.css
    /js
        scenario.js
```
