using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Domain.Enums
{
    public enum ReportStatus
        {
            New = 1,
            Assigned = 2,
            Accepted = 3,
            InProgress = 4,
            Resolved = 5,
            Closed = 6,
            Rejected = 7,
            Cancelled = 8
        }
    }
