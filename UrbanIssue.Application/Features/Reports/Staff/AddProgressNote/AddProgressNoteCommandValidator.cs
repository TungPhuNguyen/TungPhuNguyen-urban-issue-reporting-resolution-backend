using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Staff.AddProgressNote;

public sealed class AddProgressNoteCommandValidator
    : AbstractValidator<AddProgressNoteCommand>
{
    public AddProgressNoteCommandValidator()
    {
        RuleFor(command => command.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(command => command.Note)
            .NotEmpty()
            .WithMessage("Ghi chú tiến độ không được để trống.")
            .MinimumLength(5)
            .WithMessage("Ghi chú tiến độ phải có ít nhất 5 ký tự.")
            .MaximumLength(2000)
            .WithMessage("Ghi chú tiến độ không được vượt quá 2000 ký tự.");
    }
}
