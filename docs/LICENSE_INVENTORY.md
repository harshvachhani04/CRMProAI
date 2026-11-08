# Third-Party License Inventory

This document tracks third-party frameworks, libraries, and tools used in **CRMPro AI** to ensure license compliance (commercial, portfolio, and open-source compatibility).

---

## 1. Backend Libraries (.NET 10 / C#)

| Package Name | Purpose / Layer | License | Compliance Status |
|---|---|---|---|
| **Microsoft.AspNetCore.App** | ASP.NET Core 10 Web API Framework | MIT | Approved |
| **Npgsql.EntityFrameworkCore.PostgreSQL** | EF Core 10 Provider for PostgreSQL | PostgreSQL License (BSD-like) | Approved |
| **Pgvector.EntityFrameworkCore** | pgvector extension support for EF Core | MIT | Approved |
| **Microsoft.Extensions.AI** | Unified AI / LLM / Embedding abstractions | MIT | Approved |
| **FluentValidation.AspNetCore** | Application command & query validation | Apache-2.0 | Approved |
| **MediatR** | CQRS In-Process Mediator | Apache-2.0 | Approved |
| **Serilog.AspNetCore** | Structured logging & OpenTelemetry enrichment | Apache-2.0 | Approved |
| **ClosedXML** | Tabular Excel (.xlsx) report generation | MIT | Approved |
| **RabbitMQ.Client** | Message broker client for Outbox pattern | Apache-2.0 / MPL 2.0 | Approved |
| **StackExchange.Redis** | Redis distributed caching provider | MIT | Approved |
| **xUnit** | Unit testing framework | Apache-2.0 | Approved |
| **FluentAssertions** | Test assertion library | Apache-2.0 | Approved |
| **Testcontainers.PostgreSql** | Integration testing container runner | MIT | Approved |
| **NetArchTest.Rules** | Clean Architecture boundary verification | MIT | Approved |

---

## 2. Frontend Libraries (Angular 20+ / TypeScript)

| Package Name | Purpose / Layer | License | Compliance Status |
|---|---|---|---|
| **@angular/core** | Angular Web Framework | MIT | Approved |
| **primeng** | UI Component Library (Grids, Dialogs, Menus) | MIT | Approved |
| **primeicons** | Icon set for PrimeNG components | MIT | Approved |
| **tailwindcss** | Utility-first CSS framework | MIT | Approved |
| **rxjs** | Reactive extensions for event streams | Apache-2.0 | Approved |
| **lucide-angular** | Feather/Lucide vector icons | ISC (MIT-equivalent) | Approved |
| **chart.js** | Visual charting for dashboards | MIT | Approved |

---

## 3. License Policy

1. **Permitted Licenses**: MIT, Apache-2.0, BSD-2-Clause, BSD-3-Clause, ISC.
2. **Prohibited Licenses**: GPL v2/v3, AGPL v3, SSPL, or any license requiring reciprocal open-sourcing of proprietary business code.
3. **No Unlicensed Assets**: All fonts, icons, templates, and vector graphics must carry verifiable permissive licenses.
