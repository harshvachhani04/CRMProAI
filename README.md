# CRMPro AI

> **Enterprise-grade Multi-Tenant B2B CRM for IT Services and Software Development Firms**  
> Built with Clean Architecture, Angular 20+, ASP.NET Core 10 Web API, PostgreSQL + pgvector, and Microsoft.Extensions.AI.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-20%2B-red.svg)](https://angular.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16%2B%20%2B%20pgvector-blue.svg)](https://github.com/pgvector/pgvector)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20%2B%20Modular%20Monolith-brightgreen.svg)]()

---

## 📖 Overview

**CRMPro AI** is designed specifically for small to medium-sized IT services companies (5–200 internal users) that manage high-touch, consultative B2B relationships. The platform manages the entire customer lifecycle from first enquiry to post-sale support while embedding permission-aware, grounded AI assistance.

### Key Capabilities
- **Lead & Opportunity Pipeline**: Multi-stage deal management, qualification checklists, automated duplicate detection, and atomic lead conversion.
- **Account 360 & Timeline**: Consolidated organization view linking contacts, deals, proposals, meetings, and support tickets in an auditable chronological timeline.
- **Proposals & Quotations**: Version-controlled commercial proposals with customizable sections, immutable sent versions, and formal approval flows.
- **Support & Customer Portal**: Multi-queue ticket management, SLA tracking with business calendar calculations, customer-facing portal, and separation of public vs internal notes.
- **Knowledge Base & RAG**: Controlled document ingestion (PDF/DOCX/TXT), semantic chunking, and PostgreSQL pgvector hybrid search.
- **AI Platform & Workflows**: Grounded RAG Q&A, natural language CRM queries, bounded multi-step agent workflows with cryptographic approval tokens, and strict human oversight.
- **Enterprise Multi-Tenancy**: Logical tenant isolation via `TenantId` query filters, unique composite indexes, and PostgreSQL Row-Level Security (RLS) defense in depth.

---

## 🏛️ Architectural Blueprint

CRMPro AI follows **Clean Architecture** principles within a **Modular Monolith** pattern:

```
src/
├── CRMProAI.Domain/            # Entities, Value Objects, Domain Events, Invariants (Zero external dependencies)
├── CRMProAI.Application/       # Use Cases, CQRS Commands/Queries, Validators, Ports/Interfaces
├── CRMProAI.Infrastructure/    # EF Core 10, PostgreSQL + pgvector, Redis, RabbitMQ Outbox, AI Adapters
├── CRMProAI.Contracts/         # Stable cross-boundary DTOs and contracts
├── CRMProAI.Worker/            # Background Worker Service (Ingestion, SLA Clocks, Reminders)
└── CRMProAI.Api/               # ASP.NET Core 10 REST Web API (/api/v1, Swagger, ProblemDetails)

tests/
├── CRMProAI.Domain.UnitTests/
├── CRMProAI.Application.UnitTests/
├── CRMProAI.Infrastructure.IntegrationTests/
├── CRMProAI.Api.IntegrationTests/
└── CRMProAI.ArchitectureTests/

web/
├── crmpro-ai-web/              # Main CRM Web Application (Angular 20+, PrimeNG, Tailwind CSS)
└── crmpro-ai-portal/           # Customer Portal Web Application (Lightweight, Isolated scope)

deploy/
├── docker/                     # Docker Compose configurations (Postgres, Redis, RabbitMQ)
└── scripts/                    # Database migration & seed scripts

docs/                           # Architecture Decision Records (ADRs), Runbooks, Phase Trackers
Spec/                           # Business Requirements (BRD) & Technical Developer Specification
```

---

## 🛠️ Technology Stack

| Layer | Selected Technology | Details / Notes |
|---|---|---|
| **Frontend** | Angular 20+ (Signals + RxJS) | Standalone components, Reactive Forms, PrimeNG UI, Tailwind CSS |
| **Customer Portal**| Angular 20+ | Distinct scoped application for external customers |
| **Backend API** | ASP.NET Core 10 Web API | REST `/api/v1`, OpenAPI/Swagger, RFC 7807 ProblemDetails |
| **Architecture** | Clean Architecture + CQRS | Explicit command/query handlers, FluentValidation |
| **Database** | PostgreSQL + EF Core 10 | Code First migrations, UUID keys, UTC timestamps, `xmin` concurrency |
| **Vector Search** | pgvector extension | Hybrid dense vector + PostgreSQL full-text keyword retrieval |
| **Caching** | Redis | Multi-tenant scoped cache keys with explicit invalidation |
| **Messaging** | RabbitMQ + Outbox Pattern | Reliable asynchronous event publication and background processing |
| **AI Integration** | Microsoft.Extensions.AI | Provider-neutral abstractions (`IChatClient`, `IEmbeddingGenerator`) |
| **Testing** | xUnit, Testcontainers, Playwright | Unit tests, database integration tests, architecture guard tests |

---

## 🚦 Micro-Phase Implementation Roadmap

The implementation is executed across **108 discrete micro-phases (P000–P107)** with strict definition of done gates:

- **P000–P008**: Foundation, Baseline Scaffold, ADRs, Docker & EF Core Baseline
- **P009–P025**: Multi-Tenancy, Identity, Role Matrix, Settings, Audit & Storage
- **P026–P034**: Lead Management, Duplicate Detection & Conversion Wizard
- **P035–P044**: Accounts, Contacts & Opportunity Pipeline Kanban
- **P045–P050**: Activities, Task Reminders & Proposal Management Lifecycle
- **P051–P053**: Executive & Operational Role-Based Dashboards
- **P054–P061**: Customer Support Tickets, SLA Monitoring & Customer Portal
- **P062–P071**: Knowledge Base, Document Ingestion & pgvector RAG Search
- **P072–P094**: AI Gateway, RAG Generation, Agent Workflows & Approval UX
- **P095–P107**: Reporting, Notifications, Security Audit, E2E Testing & Demo Handover

Track real-time progress in [docs/PHASE_TRACKER.md](docs/PHASE_TRACKER.md).

---

## 💻 Prerequisites & Local Setup

### System Prerequisites
- **.NET 10 SDK** (v10.0.200+)
- **Node.js** (v22.x or v24.x LTS) & **npm** (v11.x+)
- **Angular CLI** (v20+)
- **PostgreSQL 16+** with `pgvector`
- **RabbitMQ** (Local service or Docker)
- **Redis**

### Quick Start
```powershell
# 1. Clone repository
git clone https://github.com/harshvachhani04/CRMProAI.git
cd CRMProAI

# 2. Build backend solution (once scaffolded)
dotnet build

# 3. Run frontend
cd web/crmpro-ai-web
npm install
npm start
```

---

## 📄 License & Standards

- **License**: [MIT](LICENSE)
- **Branching Strategy**: [docs/BRANCHING_STRATEGY.md](docs/BRANCHING_STRATEGY.md)
- **Coding Standards**: [docs/CODING_STANDARDS.md](docs/CODING_STANDARDS.md)
- **License Inventory**: [docs/LICENSE_INVENTORY.md](docs/LICENSE_INVENTORY.md)
