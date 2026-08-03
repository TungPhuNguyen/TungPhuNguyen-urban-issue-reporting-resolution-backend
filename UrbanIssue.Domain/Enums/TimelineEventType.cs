namespace UrbanIssue.Domain.Enums;

public enum TimelineEventType
{
    ReportCreated = 1,
    StatusChanged = 2,
    ProgressNoteAdded = 3,
    ProgressImagesUploaded = 4,
    ComplaintSubmitted = 5,
    ComplaintAccepted = 6,
    ComplaintRejected = 7,
    CategoryChanged = 8,
    ReportEdited = 9,
    ReportCancelled = 10,
    SLAWarning = 11,
    SLABreached = 12,
    SLAEscalated = 13
}
