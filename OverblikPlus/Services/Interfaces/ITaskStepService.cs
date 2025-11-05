using OverblikPlus.Common;
using OverblikPlus.Models.Dtos.TaskSteps;

namespace OverblikPlus.Services.Interfaces
{
    public interface ITaskStepService
    {
        Task<Result<List<ReadTaskStepDto>>> GetStepsForTask(int taskId);
        Task<Result<ReadTaskStepDto>> GetTaskStep(int taskId, int stepId);
        Task<Result> CreateTaskStep(CreateTaskStepDto newStep);
        Task<Result> UpdateTaskStep(int taskId, int stepId, UpdateTaskStepDto updatedStep);
        Task<Result> DeleteTaskStep(int taskId, int stepId);
    }
}