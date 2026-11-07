# Architecture Decision Records (ADRs)

This directory documents the architectural decision records (ADRs) for the `cloud-work` repository. We record significant architectural choices, technology selections, and structural patterns to preserve context, explain trade-offs, and align human developers and autonomous AI agents.

## ADR Index

| ADR | Title | Status | Date | Area |
| :---: | :--- | :---: | :---: | :--- |
| [0001](0001-record-architecture-decisions.md) | [Record Architecture Decisions](0001-record-architecture-decisions.md) | `Accepted` | 2026-11-07 | Architecture Governance & Standards |
| [0002](0002-aws-serverless-architecture-and-dependency-modernization.md) | [AWS Serverless Architecture and Dependency Modernization](0002-aws-serverless-architecture-and-dependency-modernization.md) | `Accepted` | 2026-11-07 | Cloud Architecture & Dependencies |

---

## ADR Process & Standards

We adhere to the Architecture Decision Record principles defined in the `documentation-and-adrs` standard:

### Structure of an ADR
Each ADR must follow the standard MADR / Nygard format:
1. **Title**: `# ADR-NNNN: Title`
2. **Status**: `Proposed` \| `Accepted` \| `Superseded by ADR-XXXX` \| `Deprecated`
3. **Date**: `YYYY-MM-DD`
4. **Context**: Background, problem definition, constraints, and requirements.
5. **Decision**: Chosen approach, implementation details, and technical rationale.
6. **Alternatives Considered**: Competing alternatives evaluated with explicit pros/cons and rejection rationale.
7. **Consequences**: Positive benefits, trade-offs, operational considerations, and downstream impacts.

### Lifecycle
```
PROPOSED → ACCEPTED → (SUPERSEDED by ADR-XXXX or DEPRECATED)
```
- **Immutability**: Once accepted, ADRs are not deleted or rewritten to represent new directions. They serve as an immutable historical record.
- **Superseding**: When an architectural decision evolves or is replaced, author a new ADR and update the prior record's status to `Superseded by ADR-XXXX` with a link to the new record.
