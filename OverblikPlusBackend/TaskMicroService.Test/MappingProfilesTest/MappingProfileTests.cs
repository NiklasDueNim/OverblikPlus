using AutoMapper;
using TaskMicroService.Profiles;

namespace TaskMicroService.Test.MappingProfilesTest;

public class MappingProfileTests
{
    [Fact]
    public void Configuration_ShouldBeValid()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        configuration.AssertConfigurationIsValid();
    }
}