using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Domain.Enums
{
    public enum NotificationType
    {
        ReportAssigned = 1,
        ReportStatusChanged = 2,
        ReportResolved = 3,
        ReportClosed = 4,
        ReportRejected = 5,
        ReportReopened = 6,
        ReportReassigned = 7,
        ComplaintSubmitted = 8,
        SLAWarning = 9,
        SLABreached = 10,
        Escalated = 11
    }
}
