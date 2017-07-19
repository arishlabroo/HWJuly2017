using System.Threading.Tasks;
using TaskEntity;

namespace TaskDomain
{
    public interface IScheduleTaskDomain
    {
        Task<int> Schedule(ScheduleTask scheduleTask);
    }
}