namespace CedearLedger.Domain.Models;

public sealed record ProfitAndLoss(
    decimal? PnLArs,
    UsdPnl? PnLUsdMep,
    UsdPnl? PnLUsdCcl,
    bool IsComplete
);
