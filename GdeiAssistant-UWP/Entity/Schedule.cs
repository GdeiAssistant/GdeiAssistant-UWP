namespace GdeiAssistant.Entity
{
    public class Schedule
    {
        public string id { set; get; }

        public int? scheduleLength { set; get; }

        public string scheduleName { set; get; }

        public string scheduleType { set; get; }

        public string scheduleLesson { set; get; }

        public string scheduleTeacher { set; get; }

        public string scheduleLocation { set; get; }

        public string colorCode { set; get; }

        public int? position { set; get; }

        public int? row { set; get; }

        public int? column { set; get; }

        public int? minScheduleWeek { set; get; }

        public int? maxScheduleWeek { set; get; }
    }
}
