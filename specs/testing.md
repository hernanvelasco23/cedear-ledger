# Testing Specs

This document defines the **testing strategy and minimum guarantees**
for correctness, especially for financial calculations.

---

## 1. Core Principle

> Financial correctness is more important than implementation elegance.

Any change that affects calculations MUST be protected by tests.

---

## 2. Mandatory Test Categories

### 2.1 Calculation Engine Tests (Critical)

The calculation engine MUST be covered by unit tests
that validate compliance with `specs/calculations.md`.

Mandatory scenarios:

1. USD calculations use **SELL rate only**
2. Historical DollarRate fallback uses:
   - closest **previous business day**
   - never future dates
3. Missing DollarRate → result marked as `Incomplete`
4. Missing CEDEAR price → result marked as `Incomplete`
5. MEP and CCL calculations:
   - are independent
   - are never mixed
6. Output always includes:
   - DollarType
   - Rate value
   - RateDate

These tests are **non-negotiable**.

---

## 3. Application Layer Tests

Handlers SHOULD be covered by unit tests that validate:

- Correct upsert behavior (replace on same key)
- No duplication on unique keys
- Correct persistence interaction
- No business calculations inside handlers

Mocks or in-memory DBs are acceptable.

---

## 4. API Layer Tests (Optional for MVP)

API tests MAY include:
- Endpoint wiring
- HTTP status codes
- Basic request/response shape

API tests are less critical than calculation tests.

---

## 5. Regression Policy

- Any bug found in production MUST:
  1. Be reproduced with a failing test
  2. Be fixed
  3. Have the test committed with the fix

---

## 6. Coverage Expectations

- Calculation Engine:
  - 100% coverage on core scenarios
- Application handlers:
  - Coverage on happy path + key edge cases
- Infrastructure:
  - Coverage optional (tested indirectly)

---

## 7. CI Expectations

- Tests MUST run in CI
- A failing test blocks merges
- Financial correctness > speed

---

## 8. Philosophy

Tests are not documentation.
Specs are documentation.
Tests are enforcement.
