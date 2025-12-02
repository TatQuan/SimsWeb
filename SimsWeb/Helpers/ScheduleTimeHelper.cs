using System;

namespace SimsWeb.Helpers
{
    public static class ScheduleTimeHelper
    {
        // Danh sách các tiết với giờ cố định
        public static readonly (int Index, TimeSpan Start, TimeSpan End, string Label)[] Periods =
        {
            (1, new TimeSpan(7,  15, 0), new TimeSpan(9, 15, 0), "Period 1 (07:00 - 07:55)"),
            (2, new TimeSpan(9,  25, 0), new TimeSpan(11, 25, 0), "Period 2 (08:00 - 08:55)"),
            (3, new TimeSpan(12,  0, 0), new TimeSpan(14, 0, 0), "Period 3 (09:00 - 09:55)"),
            (4, new TimeSpan(14, 10, 0), new TimeSpan(16,10, 0), "Period 4 (10:00 - 10:55)"),
            (5, new TimeSpan(16,20, 0), new TimeSpan(18,20,0), "Period 5 (12:00 - 12:55)"),
            (6, new TimeSpan(18,30, 0), new TimeSpan(20,30,0), "Period 6 (13:00 - 13:55)"),
        };

        public static (TimeSpan Start, TimeSpan End) GetPeriodTime(int index)
        {
            var p = Array.Find(Periods, x => x.Index == index);
            if (p.Index == 0)
                throw new ArgumentException($"Invalid period index: {index}");

            return (p.Start, p.End);
        }

        public static int? GetPeriodIndex(TimeSpan startTime)
        {
            var p = Array.Find(Periods, x => x.Start == startTime);
            return p.Index == 0 ? (int?)null : p.Index;
        }
    }
}
