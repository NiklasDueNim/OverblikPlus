using OverblikPlus.Shared.Common;
using TaskMicroService.dtos.TaskStep;

namespace TaskMicroService.Services.Interfaces
{
    public interface ITaskStepService
    {
        Task<Result<List<ReadTaskStepDto>>> GetStepsForTask(int taskId);
        Task<Result<int>> CreateTaskStep(CreateTaskStepDto createStepDto);
        Task<Result<ReadTaskStepDto>> GetTaskStep(int taskId, int stepId);
        Task<Result> UpdateTaskStep(int taskId, int stepId, UpdateTaskStepDto updateStepDto); 
        Task<Result> DeleteTaskStep(int taskId, int stepId);
    }
}