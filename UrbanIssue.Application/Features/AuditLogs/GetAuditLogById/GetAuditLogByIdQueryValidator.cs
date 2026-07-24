using FluentValidation;

namespace UrbanIssue.Application.Features.AuditLogs.GetAuditLogById;

public sealed class GetAuditLogByIdQueryValidator
    : AbstractValidator<GetAuditLogByIdQuery>
{
    public GetAuditLogByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID Audit Log không hợp lệ.");
    }
}
