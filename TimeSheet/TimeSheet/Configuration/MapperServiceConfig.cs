using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace TimeSheet.Configuration
{
    public static class MapperServiceConfig
    {
        public static void Configure(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(config => config.AddProfile<MapperProfile>());
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}
