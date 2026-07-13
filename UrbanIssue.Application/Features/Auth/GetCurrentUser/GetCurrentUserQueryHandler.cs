using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Auth.GetCurrentUser
{
    public sealed class GetCurrentUserQueryHandler
    : IRequestHandler<
        GetCurrentUserQuery,
        GetCurrentUserResult>
    {
        private readonly IApplicationDbContext
            _dbContext;

        public GetCurrentUserQueryHandler(
            IApplicationDbContext dbContext)
        {
            _dbContext =
                dbContext;
        }

        public async Task<GetCurrentUserResult>
            Handle(
                GetCurrentUserQuery request,
                CancellationToken cancellationToken)
        {
            var user =
                await _dbContext.Users
                    .AsNoTracking()
                    .Include(
                        user =>
                            user.Role)
                    .Include(
                        user =>
                            user.Department)
                    .SingleOrDefaultAsync(
                        user =>
                            user.Id
                                == request.UserId,
                        cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy tài khoản.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Tài khoản đã bị vô hiệu hóa.");
            }

            return new GetCurrentUserResult(
                UserId:
                    user.Id,

                FullName:
                    user.FullName,

                Email:
                    user.Email,

                PhoneNumber:
                    user.PhoneNumber,

                Role:
                    user.Role.Name,

                DepartmentId:
                    user.DepartmentId,

                DepartmentName:
                    user.Department?.Name);
        }
    }
}
