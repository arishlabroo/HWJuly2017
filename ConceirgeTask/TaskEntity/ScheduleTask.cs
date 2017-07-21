using System;

namespace TaskEntity
{
    public class ScheduleTask
    {
        public int Id { get; set; }
        public string TaskType { get; set; }
        public DateTime When { get; set; }
        public int BethId { get; set; }
    }
}
