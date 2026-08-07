using AuditCore.Application.Features.Branches.Models;

namespace AuditCore.Application.Features.Branches;

public interface IBranchService
{
    Task<IReadOnlyCollection<BranchDto>> GetAllAsync(
        Guid? organizationId = null,
        CancellationToken cancellationToken = default);

    Task<BranchDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<BranchDto> CreateAsync(
        CreateBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<BranchDto?> UpdateAsync(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
