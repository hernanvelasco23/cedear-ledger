# CEDEAR Ledger — Data Specification (v1.0)

## Scope
Defines data sources, refresh rules, caching, and fallback behavior for:
- FX rates (MEP, CCL)
- CEDEAR prices (ARS)

This spec prioritizes correctness and transparency over completeness.

---

## 1. Principles
- Data must be traceable: every value must include source + timestamp/date.
- The system must never invent or estimate missing values.
- If automated retrieval fails, the user may provide manual values (fallback B).
- Manual data must be clearly labeled as user-provided.

---

## 2. Data Types

### 2.1 FX Rates
- MEP Sell Rate
- CCL Sell Rate

For each FX rate record, the system stores:
- RateValue
- RateDate (date used for the calculation)
- RetrievedAt (timestamp)
- Source
- IsManual (boolean)

### 2.2 CEDEAR Prices
- Current CEDEAR price in ARS for a given ticker

For each price record, the system stores:
- Ticker
- PriceARS
- AsOf (timestamp)
- Source
- IsManual (boolean)

---

## 3. Single Source Policy (MVP)
For MVP, the system uses exactly one primary source per data type:
- One source for FX (MEP + CCL)
- One sourcecd for CEDEAR prices

If the primary source is unavailable or missing values:
- The system must not switch to another automated source in MVP.
- The system must use manual fallback (Section 6).

(Additional sources may be added in later versions via a new spec version.)

---

## 4. Refresh & Caching Rules

### 4.1 FX Current
- Refresh interval: every 15 minutes during market hours (configurable)
- Cache TTL: 15 minutes

### 4.2 FX Historical (by trade date)
- Retrieved on-demand:
  - When a user saves an operation
  - Or when summary is requested and missing FX exists
- Cache TTL: 30 days (historical FX rarely changes)

### 4.3 CEDEAR Prices Current
- Refresh interval: every 10 minutes during market hours (configurable)
- Cache TTL: 10 minutes

### 4.4 Market Hours (MVP)
- Market hours are considered local Argentina time.
- The app may refresh outside market hours, but must keep "AsOf" visible.

---

## 5. Business Day Fallback (FX Historical)
When an FX rate is required for a trade date and is not available:

- Allowed fallback: closest previous business day ONLY.
- Not allowed:
  - using next day
  - interpolation
  - averaging
  - any estimation

If after fallback the rate is still unavailable:
- Mark calculation as Incomplete unless user provides manual value.

---

## 6. Manual Fallback (User-Provided Data)
If automated data retrieval fails or is missing:

### 6.1 Manual FX (MEP/CCL)
The user may enter:
- Rate value
- Rate date (must be explicit)

Rules:
- Manual FX must be flagged `IsManual = true`.
- The UI must clearly label the value as "Manual".
- Manual FX overrides missing automated data for that date.

### 6.2 Manual CEDEAR Price
The user may enter:
- Price in ARS
- As-of timestamp/date

Rules:
- Manual price must be flagged `IsManual = true`.
- The UI must clearly label it as "Manual".
- Manual price overrides missing automated price.

---

## 7. Transparency Requirements (UI + API)
For every number shown or returned that depends on external data, include:
- source
- date/timestamp used
- whether it is manual

Minimum display requirements:
- For USD (MEP/CCL) results: show FX type, rate, and rate date
- For valuations: show price AsOf and source

---

## 8. Prohibited Behaviors
The system must never:
- Estimate FX or prices
- Silently switch sources in MVP
- Hide manual inputs as if they were official data
- Use a different FX rate type than the one displayed (MEP vs CCL)
