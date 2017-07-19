using System.Threading.Tasks;
using TaskModel;

namespace TaskService
{
    public interface IScheduleTaskservice
    {
        Task Schedule(ScheduleTaskDto dto);
    }
}