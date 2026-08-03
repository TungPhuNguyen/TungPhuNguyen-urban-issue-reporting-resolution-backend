using FluentValidation;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Features.Reports.PostResolution.SubmitComplaint;

public sealed class SubmitComplaintCommandValidator
    : AbstractValidator<SubmitComplaintCommand>
{
    public SubmitComplaintCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Lý do khiếu nại không được để trống.")
            .MinimumLength(10)
            .WithMessage("Lý do khiếu nại phải có ít nhất 10 ký tự.")
            .MaximumLength(2000)
            .WithMessage("Lý do khiếu nại không được vượt quá 2000 ký tự.");

        RuleFor(x => x.Images)
            .Must(images => images.Count <= 5)
            .WithMessage("Khiếu nại được tải lên tối đa 5 ảnh.");

        RuleForEach(x => x.Images)
            .Must(IsValidImage)
            .WithMessage("Ảnh khiếu nại phải là JPG, PNG hoặc WEBP và không quá 5 MB.");
    }

    private static bool IsValidImage(UploadFile file)
    {
        return file.Length is > 0 and <= 5 * 1024 * 1024
            && file.ContentType is "image/jpeg" or "image/png" or "image/webp";
    }
}
