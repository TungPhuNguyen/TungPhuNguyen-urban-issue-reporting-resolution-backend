using System;
using System.Collections.Generic;
using System.Text;

namespace UrbanIssue.Application.Common.Settings
{
    public sealed class DefaultSlaSettings
    {
        public const string SectionName = "DefaultSla";

        public int LowHours { get; init; }

        public int MediumHours { get; init; }

        public int HighHours { get; init; }
    }
}
