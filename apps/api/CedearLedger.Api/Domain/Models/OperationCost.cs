namespace CedearLedger.Domain.Models;

public sealed record OperationCost(
    Operation Operation,
    decimal ArsCost,
    UsdValueWithFx? UsdCostMep,
    UsdValueWithFx? UsdCostCcl,
    bool IsComplete
);
