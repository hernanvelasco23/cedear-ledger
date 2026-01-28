# CEDEAR Ledger — Product Specification (v1.0)

## Purpose
CEDEAR Ledger provides accurate and transparent tracking of CEDEAR
investments in Argentina, allowing users to understand historical
performance in ARS and USD using MEP and CCL exchange rates.

## Supported Instruments
- CEDEARs only

## Supported Currencies
- ARS
- USD using:
  - MEP
  - CCL

## Supported User Actions
- Manual entry of operations:
  - trade date
  - ticker
  - quantity
  - price in ARS
  - fees in ARS (optional)

## Non-goals
- No buy/sell recommendations
- No forecasts or predictions
- No portfolio optimization
- No tax calculations
- No broker integrations (MVP)

## Core Principles
- Accuracy over convenience
- Transparency over abstraction
- No inferred or estimated financial data
- All calculations must be reproducible

## User Guarantees
- USD calculations are based on historical FX rates on the trade date
  (or closest previous business day if missing).
- FX type (MEP/CCL), rate value, and rate date are always shown.
- Missing data is marked as incomplete, never estimated.

## Disclaimer
Informational tool only. Not investment advice.
