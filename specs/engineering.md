# Engineering Specs

This document defines the **engineering and architectural rules**
that must be followed in this repository.

These rules are **binding for contributors**, but are NOT part of the
business or product contract.

---

## 1. Architecture Overview

The project follows **Clean Architecture + CQRS** principles.

Layers:

- **API**
  - ASP.NET Core controllers
  - HTTP only (request/response)
  - No business logic
  - No persistence access

- **Application**
  - Use cases
  - CQRS Commands / Queries
  - MediatR handlers
  - Orchestration only

- **Domain**
  - Pure domain models
  - Value objects
  - No EF Core
  - No infrastructure dependencies

- **Infrastructure**
  - Persistence (EF Core, SQL Server)
  - External integrations
  - No business rules

---

## 2. CQRS + MediatR Rules

### Controllers
- MUST NOT reference Infrastructure or DbContext
- MUST NOT contain business logic
- MUST delegate work to MediatR
- Responsibilities:
  - HTTP binding
  - Minimal null/required validation
  - Return HTTP status codes

### Commands
- Represent **write operations**
- One command = one use case
- Example:
  - `UpsertDollarRateCommand`
  - `CreateOperationCommand`

### Queries
- Represent **read operations**
- No side effects
- Return DTOs or read models

### Handlers
- One handler per Command / Query
- Can access Infrastructure via DI
- Must not contain HTTP concerns
- Must not perform calculations already defined in the Calculation Engine

---

## 3. Domain Rules

- Domain models:
  - Must be persistence-agnostic
  - Must not reference EF Core
  - Must not contain infrastructure logic
- Domain represents **facts**, not calculated results
- Examples of allowed domain concepts:
  - Operation
  - DollarRate
  - CedearPrice

Examples of NOT allowed domain concepts:
- PnL
- Portfolio totals
- Aggregated valuations

---

## 4. Calculation Engine Rules

- Calculation Engine is:
  - Deterministic
  - Pure
  - Side-effect free
- It must:
  - Receive all required data as input
  - Never access DB or external APIs
  - Never persist anything
- All financial formulas must follow `specs/calculations.md`
- If required data is missing, result MUST be marked as `Incomplete`
- Default values (0, empty) MUST NOT be invented

---

## 5. Persistence Rules

- EF Core lives ONLY in Infrastructure
- Fluent configuration only (no data annotations in Domain)
- No computed columns
- No business logic in DbContext
- Database stores:
  - Facts only
  - Never calculated results

---

## 6. Dependency Rules (Enforced)

Allowed references:

- API → Application
- Application → Domain
- Infrastructure → Domain
- API → Infrastructure (only for DI registration)

Forbidden references:

- Domain → anything
- Application → API
- Controllers → DbContext

---

## 7. Evolution Rules

- New features MUST start in Application
- Controllers remain thin
- Specs drive behavior, not implementation details
- Refactors that preserve specs are allowed
