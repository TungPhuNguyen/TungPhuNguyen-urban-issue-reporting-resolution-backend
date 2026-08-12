using MediatR;
using UrbanIssue.Application.Features.Reports.Staff.Common;

namespace UrbanIssue.Application.Features.Reports.Staff.AddProgressNote;

public sealed record AddProgressNoteCommand(
    Guid ReportId,
    string Note)
    : IRequest<StaffProgressUpdateResult>;
