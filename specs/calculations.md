# CEDEAR Ledger — Calculation Specification (v1.0)

## Scope
Defines how performance is calculated for CEDEAR portfolios in ARS and USD
using MEP and CCL. Calculations must be deterministic and auditable.

---

## 1. Cost Basis

### 1.1 ARS Cost (per operation)
ARS_Cost = (Quantity × Price_ARS) + Fees_ARS

Fees are always included.

### 1.2 USD Cost (MEP / CCL)
USD_Cost_MEP = ARS_Cost / MEP_Sell_Rate(trade_date)  
USD_Cost_CCL = ARS_Cost / CCL_Sell_Rate(trade_date)

Rules:
- Use SELL rate only.
- Rate must correspond to the trade date.
- If missing: use closest previous business day.
- Never interpolate or estimate FX rates.

---

## 2. Current Valuation

CurrentValue_ARS = TotalQuantity × CurrentPrice_ARS

CurrentValue_USD_MEP = CurrentValue_ARS / Current_MEP_Rate  
CurrentValue_USD_CCL = CurrentValue_ARS / Current_CCL_Rate

---

## 3. Profit and Loss

PnL_ARS = CurrentValue_ARS − TotalInvested_ARS

PnL_USD_MEP = CurrentValue_USD_MEP − TotalInvested_USD_MEP  
PnL_USD_CCL = CurrentValue_USD_CCL − TotalInvested_USD_CCL

---

## 4. Weighted Averages

AvgPrice_ARS = Σ(Quantity × Price_ARS) / Σ(Quantity)

Weighted_MEP = TotalInvested_ARS / TotalInvested_USD_MEP  
Weighted_CCL = TotalInvested_ARS / TotalInvested_USD_CCL

---

## 5. Missing Data
- If FX rate is missing after applying “previous business day” fallback:
  mark results as Incomplete.
- Do not return 0, do not guess.

---

## 6. Display Requirements
For every USD figure, display:
- FX type (MEP/CCL)
- FX rate value
- FX rate date
