using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Auth.GetCurrentUser;

namespace UrbanIssue.Application.Features.Auth.UpdateProfile;

public sealed class UpdateProfileCommandHandler
    : IRequestHandler<UpdateProfileCommand, GetCurrentUserResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProfileCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<GetCurrentUserResult> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .Include(item => item.Role)
            .Include(item => item.Department)
            .SingleOrDefaultAsync(
                item => item.Id == _currentUserService.UserId && item.IsActive,
                cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("Không tìm thấy tài khoản.");
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new GetCurrentUserResult(
            user.Id,
            user.FullName,
            user.Email,
            user.EmailVerifiedAt.HasValue,
            user.PhoneNumber,
            user.Role.Name,
            user.DepartmentId,
            user.Department?.Name);
    }
}
