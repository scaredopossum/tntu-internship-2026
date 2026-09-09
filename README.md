# TNTU Internship 2026 — Team Task Board

A microservices-based task board built with **ASP.NET Core**, **EF Core**, **Azure Cosmos DB**, and **GitHub Actions** CI/CD.

The system consists of two cooperating Web APIs — **Projects.Api** and **Tasks.Api** — deployed to Azure App Service and integrated with Azure Cosmos DB.

---

## Architecture Overview

| Service | Responsibility |
|---------|----------------|
| **Projects.Api** | Create, list, update, and archive projects |
| **Tasks.Api** | Manage tasks within projects; validates project existence via HTTP |

```mermaid
flowchart LR
  Client[API Client] --> ProjectsApi[Projects.Api]
  Client --> TasksApi[Tasks.Api]
  TasksApi -->|validate project| ProjectsApi
  ProjectsApi --> Cosmos[(Cosmos DB)]
  TasksApi --> Cosmos

---

## Documentation

Start here based on your role:

| Document | Audience | Description |
|----------|----------|-------------|
| [Development Prerequisites](docs/prerequisites/development-prerequisites.md) | Students (Day 1) | Software to install, Azure/GitHub accounts, environment variables |
| [Architecture and Tech Stack](docs/architecture/architecture-and-tech-stack.md) | Students, mentors | System design, conventions, ADRs, learning links |
| [System Overview](docs/domain/system-overview.md) | Students, mentors | Domain model, business rules, entity definitions |
| [One-Month Schedule](docs/internship-plan/one-month-schedule.md) | Students, mentors | Week-by-week plan, demo script, grading rubric |
| [User Stories](docs/user-stories/README.md) | Students | 18 user stories with acceptance criteria and API contracts |

---


---

## Tech stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 8 |
| API framework | ASP.NET Core Web API |
| ORM | Entity Framework Core + Cosmos DB provider |
| Database | Azure Cosmos DB (free tier) |
| Hosting | Azure App Service F1 |
| CI/CD | GitHub Actions |
| Testing | xUnit |
| Optional | Docker, Docker Compose |

Full details and documentation links: [Architecture and Tech Stack](docs/architecture/architecture-and-tech-stack.md).

---

## Prerequisites

- .NET 8 SDK
- Azure Cosmos DB Emulator OR Azure Cosmos DB connection string
- Git

---

## Configuration

Ensure the following configuration settings or environment variables are configured for local execution:

Projects.Api (src/Projects.Api/appsettings.Development.json):

```
{
  "CosmosDb": {
    "ConnectionString": "<your-cosmosdb-connection-string>",
    "DatabaseName": "TaskBoardDb"
  }
}
```

Tasks.Api (src/Tasks.Api/appsettings.Development.json):

```
{
  "CosmosDb": {
    "ConnectionString": "<your-cosmosdb-connection-string>",
    "DatabaseName": "TaskBoardDb"
  },
  "Services": {
    "ProjectsApiUrl": "http://localhost:5000"
  }
}
```

---

## Repository structure

```
.
├── .github/
│   └── workflows/
│       ├── projects-ci.yml
│       └── tasks-ci.yml
├── src/
│   ├── Projects.Api/
│   ├── Projects.Api.Tests/
│   ├── Tasks.Api/
│   └── Tasks.Api.Tests/
├── docs/
└── README.md
```

---

## Execution

1. Start Projects.Api:

```
dotnet run --project src/Projects.Api/Projects.Api.csproj --urls "http://localhost:5000"
```

2. Start Tasks.Api (in a separate terminal):

```
dotnet run --project src/Tasks.Api/Tasks.Api.csproj --urls "http://localhost:5001"
```

3. Endpoints:

- Projects API Swagger: http://localhost:5000/swagger

- Projects API Health: http://localhost:5000/health

- Tasks API Swagger: http://localhost:5001/swagger

- Tasks API Health: http://localhost:5001/health

---

## Known limitations

1. Authentication & Authorization:
  Endpoints do not enforce JWT or API Key authentication; all routes are publicly accessible within the current MVP specification.

2. Free Tier Latency & Cold Starts:
  Azure App Service (F1) and Azure Cosmos DB Free Tier instances experience cold start delays when idle and have Request Unit (RU/s) throughput limits under high concurrency.

3. Synchronous Cross-Service HTTP Dependency:
  Tasks.Api validates ProjectId existence synchronously over HTTP via Projects.Api. If Projects.Api experiences latency or failure, dependent task operations return 502 Bad Gateway.  

4. No Cascading Deletion / Saga Management:
  Task hard deletion is supported per task. Archiving or deleting a project does not perform asynchronous cascading operations on associated task documents across Cosmos DB partitions.

---

## License and usage

This project is intended for educational use at TNTU. Mentors may adapt documentation and scope as needed.
