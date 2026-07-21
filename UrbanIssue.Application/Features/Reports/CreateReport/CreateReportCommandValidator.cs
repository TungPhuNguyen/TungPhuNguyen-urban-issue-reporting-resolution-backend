using FluentValidation;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Features.Reports.CreateReport;

public sealed class CreateReportCommandValidator
    : AbstractValidator<CreateReportCommand>
{
    private const long MaximumImageSize =
        5 * 1024 * 1024;

    private static readonly HashSet<string>
        AllowedContentTypes =
        new(
            StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public CreateReportCommandValidator()
    {
        RuleFor(command => command.CategoryId)
            .GreaterThan(0)
            .WithMessage(
                "ID loại sự cố phải lớn hơn 0.");

        RuleFor(command => command.AreaId)
            .GreaterThan(0)
            .WithMessage(
                "ID khu vực phải lớn hơn 0.");

        RuleFor(command => command.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                "Mô tả sự cố không được để trống.")
            .MinimumLength(10)
            .WithMessage(
                "Mô tả sự cố phải có ít nhất 10 ký tự.")
            .MaximumLength(2000)
            .WithMessage(
                "Mô tả sự cố không được vượt quá 2000 ký tự.");

        RuleFor(command => command.AddressText)
            .MaximumLength(500)
            .WithMessage(
                "Địa chỉ không được vượt quá 500 ký tự.")
            .When(command =>
                !string.IsNullOrWhiteSpace(
                    command.AddressText));

        RuleFor(command => command.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage(
                "Vĩ độ phải nằm trong khoảng từ -90 đến 90.");

        RuleFor(command => command.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage(
                "Kinh độ phải nằm trong khoảng từ -180 đến 180.");

        RuleFor(command => command.Images)
            .NotNull()
            .WithMessage(
                "Danh sách ảnh không được để trống.")
            .Must(images =>
                images is { Count: >= 1 and <= 5 })
            .WithMessage(
                "Báo cáo phải có từ 1 đến 5 ảnh.");

        RuleForEach(command => command.Images)
            .SetValidator(
                new ReportImageValidator(
                    MaximumImageSize,
                    AllowedContentTypes));
    }

    private sealed class ReportImageValidator
        : AbstractValidator<UploadFile>
    {
        public ReportImageValidator(
            long maximumImageSize,
            HashSet<string> allowedContentTypes)
        {
            RuleFor(file => file.FileName)
                .NotEmpty()
                .WithMessage(
                    "Tên file ảnh không hợp lệ.");

            RuleFor(file => file.Length)
                .GreaterThan(0)
                .WithMessage(
                    "File ảnh không được rỗng.")
                .LessThanOrEqualTo(
                    maximumImageSize)
                .WithMessage(
                    "Mỗi ảnh không được vượt quá 5 MB.");

            RuleFor(file => file.ContentType)
                .Must(contentType =>
                    allowedContentTypes.Contains(
                        contentType))
                .WithMessage(
                    "Chỉ hỗ trợ ảnh JPG, PNG và WEBP.");
        }
    }
}
