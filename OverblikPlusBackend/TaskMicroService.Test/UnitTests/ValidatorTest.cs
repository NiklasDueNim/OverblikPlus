using TaskMicroService.dto;
using TaskMicroService.Validators;

namespace TaskMicroService.Test.UnitTests;

public class ValidatorTest
{
    [Fact]
    public void Validator_Should_Have_Validation_Error_When_Name_Is_Empty()
    {
        var validator = new CreateTaskDtoValidator();
        var taskDto = new CreateTaskDto()
        {
            Name = "",
            Description = "Test description"
        };

        var result = validator.Validate(taskDto);
        
       Assert.False(result.IsValid, "Validation result should be invalid."); 
       
       Assert.Contains(result.Errors, e => e.ErrorMessage == "Name is required.");
    }
    
    [Fact]                                                                                      
    public void Validation_Result_Should_Be_Valid()                     
    {                                                                                           
        var validator = new CreateTaskDtoValidator();                                           
        var taskDto = new CreateTaskDto()                                                       
        {                                                                                       
            Name = "Example",                                                                          
            Description = "Test description"                                                    
        };                                                                                      
                                                                                            
        var result = validator.Validate(taskDto);                                               
                                                                                            
        Assert.True(result.IsValid, "Validation result should be valid.");                                       
    }                                                                                           
}