using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.Login
{
    public sealed class LoginCommandValidator
    : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(
                    "Email không được để trống.")
                .EmailAddress()
                .WithMessage(
                    "Email không đúng định dạng.")
                .MaximumLength(255)
                .WithMessage(
                    "Email không được vượt quá 255 ký tự.");

            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(
                    "Mật khẩu không được để trống.")
                .MaximumLength(100)
                .WithMessage(
                    "Mật khẩu không được vượt quá 100 ký tự.");
        }
    }
}
