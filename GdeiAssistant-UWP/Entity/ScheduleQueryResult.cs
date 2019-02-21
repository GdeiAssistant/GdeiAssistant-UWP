using System.Collections.Generic;

namespace GdeiAssistant.Entity
{
    public class ScheduleQueryResult
    {
        public List<Schedule> scheduleList { set; get; }

        public int? week { set; get; }
    }
}
