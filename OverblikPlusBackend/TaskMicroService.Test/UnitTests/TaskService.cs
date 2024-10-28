using AutoMapper;
using TaskMicroService.dto;
using TaskMicroService.Entities;
using TaskMicroService.Profiles;

namespace TaskMicroService.Test.UnitTests;

public class TaskService
{
    [Fact]
    public void Mapping_Profile_Maps_TaskStepDto_To_TaskStep_Correctly()
    {
        // Arrange
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = config.CreateMapper();

        var dto = new TaskStepDto
        {
            Id = 1,
            Image = "https://example.com/image.png",
            Text = "Test Step",
            StepNumber = 1
        };

        // Act
        var entity = mapper.Map<TaskStep>(dto);

        // Assert
        Assert.Equal(dto.Id, entity.TaskId);
        //Assert.Equal(dto.Image, entity.Image);
        Assert.Equal(dto.Text, entity.Text);
        Assert.Equal(dto.StepNumber, entity.StepNumber);
    }

}