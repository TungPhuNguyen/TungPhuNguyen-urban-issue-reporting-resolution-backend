using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.RefreshAccessToken
{
    public sealed class RefreshAccessTokenCommandValidator
    : AbstractValidator<RefreshAccessTokenCommand>
    {
        public RefreshAccessTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(
                    "Refresh token không được để trống.")
                .Must(
                    refreshToken =>
                        !string.IsNullOrWhiteSpace(
                            refreshToken))
                .WithMessage(
                    "Refresh token không hợp lệ.");
        }
    }
}
