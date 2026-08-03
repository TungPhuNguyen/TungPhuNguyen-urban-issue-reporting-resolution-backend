using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Reports.CheckDuplicateReports;

public sealed class CheckDuplicateReportsCommandHandler
    : IRequestHandler<CheckDuplicateReportsCommand, CheckDuplicateReportsResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IReportDuplicateChecker _duplicateChecker;

    public CheckDuplicateReportsCommandHandler(
        IApplicationDbContext dbContext,
        IReportDuplicateChecker duplicateChecker)
    {
        _dbContext = dbContext;
        _duplicateChecker = duplicateChecker;
    }

    public async Task<CheckDuplicateReportsResult> Handle(
        CheckDuplicateReportsCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == request.CategoryId,
                cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy loại sự cố có ID {request.CategoryId}.");
        }

        if (!category.IsActive)
        {
            throw new ConflictException(
                "Không thể kiểm tra phản ánh trùng với loại sự cố đã ngừng hoạt động.");
        }

        return await _duplicateChecker.CheckAsync(
            request.CategoryId,
            request.Latitude,
            request.Longitude,
            cancellationToken);
    }
}
