# ADR-008: Export Strategy — ClosedXML (.xlsx Only) with Formula Injection Defense

## Status
**Accepted** (2026-11-08)

## Context
Commercial CRM users frequently export filtered views of leads, pipeline opportunities, customer lists, and support SLA performance for executive reviews and external meetings.

Key challenges:
- Business requirement specifies: **Excel (.xlsx) only for user-facing tabular exports. No CSV/PDF export unless explicitly approved later.**
- Many software products export CSV files vulnerable to CSV/Formula Injection attacks (where cells starting with `=`, `+`, `-`, `@` execute arbitrary commands when opened in desktop spreadsheet software like Microsoft Excel).
- Exports must respect the current user's tenant boundary and record-level permissions (e.g., sales executives cannot export another team's leads).

## Decision
We implement a dedicated **Excel Export Service** based on **ClosedXML** (.NET library, MIT licensed):

1. **Format Restriction**:
   - Only native Office Open XML (`.xlsx`) format is generated for user-facing tabular exports.
   - Files include proper MIME types (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`), stylized header bands, auto-fitted columns, and generation metadata (timestamp, tenant name, active filter summary).
2. **Formula Injection Sanitization**:
   - Every string cell value is inspected before writing.
   - Any value beginning with `=`, `+`, `-`, `@`, `\t`, or `\r` is escaped by prefixing a single quote `'` or treated as plain text literal, neutralizing executable formulas.
3. **Server-Side Authorization & Filter Re-Evaluation**:
   - The export endpoint does **not** accept raw row IDs from the client.
   - Instead, the server receives the current filter criteria, validates export permissions (`*.export`), re-executes the query within the authenticated user's tenant and record security scope, and streams the authorized dataset.
4. **Asynchronous Execution for Large Datasets**:
   - Small exports (under 1,000 rows) stream directly.
   - Large exports are queued as background jobs (`ExportJob`) processed by the Worker, with temporary signed download URLs that expire after 24 hours.

## Consequences

### Positive
- Delivers polished, ready-to-use Excel workbooks formatted for business presentations.
- Eliminates formula injection vulnerabilities entirely.
- Fully audited: every export request records user ID, tenant ID, row count, filter criteria, and timestamp.

### Negative / Trade-offs
- Generating `.xlsx` binary streams requires slightly higher CPU and memory overhead compared to raw text CSVs; mitigated by async background processing for larger exports.
