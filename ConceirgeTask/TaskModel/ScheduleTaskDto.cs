using System;

namespace TaskModel
{
    public enum TaskType
    {
        CreateCsat,
        CsatEmail,
        CsatSms
    }

    public class ScheduleTaskDto
    {
        public TaskType TaskType { get; set; }
        public DateTime When { get; set; }
        public int BethId { get; set; }
    }
}