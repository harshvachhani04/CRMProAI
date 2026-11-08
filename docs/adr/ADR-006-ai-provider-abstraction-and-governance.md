# ADR-006: AI Architecture — Microsoft.Extensions.AI, Prompt Registry & Human-in-the-Loop Governance

## Status
**Accepted** (2026-11-08)

## Context
CRMPro AI provides generative AI assistance (RAG knowledge search, ticket triage and replies, deal risk analysis, and agent workflows).
Key risks:
- Vendor lock-in to a single commercial LLM provider.
- LLM hallucinations inventing proposal terms, technical fixes, or SLA promises.
- Autonomous AI execution causing irreversible damage or unverified customer communications.
- Prompt injection embedded in uploaded customer documents overriding system prompts.

## Decision
We implement a **provider-neutral, bounded AI assistance architecture**:

1. **Provider Neutrality via `Microsoft.Extensions.AI`**:
   - Application logic interfaces strictly with Microsoft's official abstractions: `IChatClient` and `IEmbeddingGenerator<string, Embedding<float>>`.
   - Domain and Application layers have zero dependencies on vendor-specific SDKs (e.g., Azure OpenAI, Google Gemini, Anthropic, Ollama). Provider adapters reside solely in `CRMProAI.Infrastructure`.
2. **Versioned Prompt Registry**:
   - All system and task prompts are maintained in a structured registry (`AiPrompt` and `AiPromptVersion`). No prompt strings scattered across controllers.
   - Prompts enforce untrusted data boundaries: user input and retrieved document chunks are delimited as untrusted data that cannot override system guidelines.
3. **Structured CRM Tools (No Raw SQL)**:
   - LLMs can only invoke a curated, typed catalog of tools (e.g., `SearchOpportunities`, `SearchLeads`, `GetAccountSummary`).
   - The model is **never** permitted to generate or execute arbitrary SQL queries against the database.
4. **Human-in-the-Loop Approval State Machine**:
   - Read-only tools execute within user permission scopes.
   - Consequential operations (creating tasks, drafting external emails, changing deal stages) produce an `AiPendingAction` in state `AwaitingApproval`.
   - State transition: `Draft -> Validating -> AwaitingApproval -> Approved -> Executing -> Completed / Failed / Rejected`.
   - Execution requires a single-use expiring approval token scoped to the SHA-256 hash of the proposed payload. If the underlying record changed in the interim, execution is safely rejected.
5. **Clear Attribution & Abstention**:
   - AI outputs are explicitly labeled as generated/suggested.
   - Grounded RAG answers must cite source document title, version, and section/page.
   - If insufficient authorized evidence exists, the AI must explicitly abstain rather than speculate.

## Consequences

### Positive
- Total freedom to switch or route between model providers based on cost, latency, or privacy terms.
- Robust protection against unauthorized mutations and prompt injection exploits.
- Full auditability: every tool call, token count, cost estimate, and human approval is recorded.

### Negative / Trade-offs
- Two-step flow for agent mutations (preview -> approval) introduces deliberate UI interaction before execution, which is an intentional safety design.
