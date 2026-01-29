using CedearLedger.Application.Operations;

namespace CedearLedger.Application.Abstractions;

public interface IOperationRepository
{
    Task<OperationDto> CreateAsync(CreateOperationData data, CancellationToken cancellationToken);
}
