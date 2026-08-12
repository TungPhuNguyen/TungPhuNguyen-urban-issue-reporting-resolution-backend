using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Features.Categories.CreateCategory
{
    public sealed class CreateCategoryCommandValidator
    : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(command => command.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(
                    "Tên loại sự cố không được để trống.")
                .MaximumLength(100)
                .WithMessage(
                    "Tên loại sự cố không được vượt quá 100 ký tự.");

            RuleFor(command => command.Description)
                .MaximumLength(2000)
                .WithMessage(
                    "Mô tả không được vượt quá 2000 ký tự.")
                .When(
                    command =>
                        !string.IsNullOrWhiteSpace(
                            command.Description));
        }
    }
}
