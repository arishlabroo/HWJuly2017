using System.Threading.Tasks;
using TaskModel;

namespace TaskService
{
    public interface IScheduleTaskService
    {
        Task Schedule(ScheduleTaskDto dto);
    }
}