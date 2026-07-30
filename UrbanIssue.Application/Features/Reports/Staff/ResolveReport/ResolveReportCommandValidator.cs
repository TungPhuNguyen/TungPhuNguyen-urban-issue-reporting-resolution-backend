using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Staff.ResolveReport;

public sealed class ResolveReportCommandValidator
    : AbstractValidator<ResolveReportCommand>
{
    private const long MaximumFileSize =
        5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public ResolveReportCommandValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(x => x.Note)
            .NotEmpty()
            .WithMessage("Ghi chú kết quả xử lý không được để trống.")
            .MinimumLength(5)
            .WithMessage("Ghi chú kết quả phải có ít nhất 5 ký tự.")
            .MaximumLength(2000)
            .WithMessage("Ghi chú kết quả không được vượt quá 2000 ký tự.");

        RuleFor(x => x.Images)
            .NotNull()
            .Must(images => images.Count is >= 1 and <= 5)
            .WithMessage("Cần tải từ 1 đến 5 ảnh kết quả xử lý.");

        RuleForEach(x => x.Images)
            .Must(file => file.Length > 0)
            .WithMessage("Tệp ảnh không được rỗng.")
            .Must(file => file.Length <= MaximumFileSize)
            .WithMessage("Mỗi ảnh không được vượt quá 5 MB.")
            .Must(file =>
                AllowedContentTypes.Contains(
                    file.ContentType.ToLowerInvariant()))
            .WithMessage("Chỉ chấp nhận ảnh JPEG, PNG hoặc WEBP.");
    }
}
