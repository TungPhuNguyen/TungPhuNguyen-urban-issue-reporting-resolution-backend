using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Auth.Register
{
    public sealed class RegisterCommandValidator
    : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.FullName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(
                    "Họ và tên không được để trống.")
                .Must(fullName =>
                    !string.IsNullOrWhiteSpace(fullName))
                .WithMessage(
                    "Họ và tên không được chỉ chứa khoảng trắng.")
                .MaximumLength(150)
                .WithMessage(
                    "Họ và tên không được vượt quá 150 ký tự.");

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
                .MinimumLength(8)
                .WithMessage(
                    "Mật khẩu phải có ít nhất 8 ký tự.")
                .MaximumLength(100)
                .WithMessage(
                    "Mật khẩu không được vượt quá 100 ký tự.")
                .Matches("[A-Z]")
                .WithMessage(
                    "Mật khẩu phải có ít nhất một chữ cái viết hoa.")
                .Matches("[a-z]")
                .WithMessage(
                    "Mật khẩu phải có ít nhất một chữ cái viết thường.")
                .Matches("[0-9]")
                .WithMessage(
                    "Mật khẩu phải có ít nhất một chữ số.")
                .Matches("[^a-zA-Z0-9]")
                .WithMessage(
                    "Mật khẩu phải có ít nhất một ký tự đặc biệt.");

            RuleFor(x => x.ConfirmPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(
                    "Xác nhận mật khẩu không được để trống.")
                .Equal(x => x.Password)
                .WithMessage(
                    "Xác nhận mật khẩu không khớp.");

            When(
                x => !string.IsNullOrWhiteSpace(
                    x.PhoneNumber),
                () =>
                {
                    RuleFor(x => x.PhoneNumber)
                        .MaximumLength(20)
                        .WithMessage(
                            "Số điện thoại không được vượt quá 20 ký tự.");
                });
        }
    }
}
