using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;

namespace UrbanIssue.Application.Features.Auth.Logout
{
    public sealed class LogoutCommandHandler
    : IRequestHandler<
        LogoutCommand,
        bool>
    {
        private readonly IApplicationDbContext
            _dbContext;

        private readonly IRefreshTokenService
            _refreshTokenService;

        public LogoutCommandHandler(
            IApplicationDbContext dbContext,
            IRefreshTokenService refreshTokenService)
        {
            _dbContext =
                dbContext;

            _refreshTokenService =
                refreshTokenService;
        }

        public async Task<bool> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            var refreshTokenHash =
                _refreshTokenService.Hash(
                    request.RefreshToken.Trim());

            var storedRefreshToken =
                await _dbContext
                    .RefreshTokens
                    .SingleOrDefaultAsync(
                        refreshToken =>
                            refreshToken.TokenHash
                                == refreshTokenHash,
                        cancellationToken);

            /*
             * Logout được thiết kế idempotent.
             *
             * Nếu token không tồn tại,
             * API vẫn xem như logout thành công.
             */
            if (storedRefreshToken is null)
            {
                return true;
            }

            /*
             * Token đã bị thu hồi trước đó
             * thì không cần cập nhật lại.
             */
            if (storedRefreshToken.RevokedAt
                is not null)
            {
                return true;
            }

            storedRefreshToken.RevokedAt =
                DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return true;
        }
    }
}
