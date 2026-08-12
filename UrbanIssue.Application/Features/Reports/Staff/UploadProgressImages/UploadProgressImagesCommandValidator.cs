using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Staff.UploadProgressImages;

public sealed class UploadProgressImagesCommandValidator
    : AbstractValidator<UploadProgressImagesCommand>
{
    private const long MaximumFileSize =
        5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public UploadProgressImagesCommandValidator()
    {
        RuleFor(command => command.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(command => command.Note)
            .MaximumLength(2000)
            .WithMessage("Ghi chú tiến độ không được vượt quá 2000 ký tự.")
            .When(command =>
                !string.IsNullOrWhiteSpace(command.Note));

        RuleFor(command => command.Images)
            .NotNull()
            .Must(images => images.Count is >= 1 and <= 5)
            .WithMessage("Cần tải từ 1 đến 5 ảnh tiến độ.");

        RuleForEach(command => command.Images)
            .Must(file => file.Length > 0)
            .WithMessage("Tệp ảnh không được rỗng.")
            .Must(file => file.Length <= MaximumFileSize)
            .WithMessage("Mỗi ảnh không được vượt quá 5 MB.")
            .Must(file =>
                !string.IsNullOrWhiteSpace(file.ContentType)
                && AllowedContentTypes.Contains(
                    file.ContentType.ToLowerInvariant()))
            .WithMessage("Chỉ chấp nhận ảnh JPEG, PNG hoặc WEBP.");
    }
}
