# Local Developer Environment Guide

This document describes how to configure, run, and manage the local backing dependencies (PostgreSQL + pgvector, Redis, and RabbitMQ) for **CRMPro AI**.

---

## 1. Quick Start with Docker Compose

The primary developer environment uses Docker Compose to provision containerized instances of all three services with health checks and persistent named volumes.

### Starting Dependencies
```powershell
# From the repository root
powershell -ExecutionPolicy Bypass -File ./deploy/scripts/start-dev-env.ps1

# Or directly with Docker Compose
cd deploy/docker
docker compose up -d
```

### Stopping Dependencies
```powershell
powershell -ExecutionPolicy Bypass -File ./deploy/scripts/stop-dev-env.ps1

# To stop and remove persistent volumes (resets databases)
powershell -ExecutionPolicy Bypass -File ./deploy/scripts/stop-dev-env.ps1 -RemoveVolumes
```

---

## 2. Service Configurations & Endpoints

| Service | Container Image | Host Port | Credentials / Defaults | Health Check |
|---|---|---|---|---|
| **PostgreSQL 16 + pgvector** | `pgvector/pgvector:pg16` | `5432` | DB: `crmpro_dev`<br>User: `crmpro_app`<br>Pass: `crmpro_dev_password_2026` | `pg_isready -U crmpro_app -d crmpro_dev` |
| **Redis 7** | `redis:7-alpine` | `6379` | Auth: `crmpro_redis_dev_2026` | `redis-cli -a <password> ping` |
| **RabbitMQ 3.13** | `rabbitmq:3.13-management-alpine` | `5672`<br>`15672` (Web UI) | User: `crmpro_user`<br>Pass: `crmpro_rabbit_dev_2026` | `rabbitmq-diagnostics -q ping` |

---

## 3. Automated Database Extensions Initialization

The PostgreSQL container executes [`deploy/docker/init-db/01-init-extensions.sql`](../deploy/docker/init-db/01-init-extensions.sql) on first boot:
- **`vector`**: Vector embedding storage and similarity operators (`<=>`, `<->`, `<#>`) for RAG.
- **`uuid-ossp`**: Standard UUID generation functions (`uuid_generate_v4()`).
- **`pg_trgm`**: Trigram matching for fast fuzzy duplicate lead and account detection.
- Creates both development database (`crmpro_dev`) and integration test database (`crmpro_test`).

---

## 4. Alternative: Bare-Metal Windows Services

If Docker is not running on the local host machine, developers can run services natively:
- **RabbitMQ**: Can be run via the Windows Service `RabbitMQ` on port `5432` / `5672`.
- **PostgreSQL 16**: Can be installed via Winget:
  ```powershell
  winget install PostgreSQL.PostgreSQL.16 --accept-package-agreements --accept-source-agreements
  ```
- **Redis on Windows**: Can be run via Memurai or WSL2.
