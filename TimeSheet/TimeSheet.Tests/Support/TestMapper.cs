using AutoMapper;
using TimeSheet.Configuration;

namespace TimeSheet.Tests.Support
{
    public static class TestMapper
    {
        public static IMapper Create()
            => new MapperConfiguration(config => config.AddProfile<MapperProfile>()).CreateMapper();
    }
}
