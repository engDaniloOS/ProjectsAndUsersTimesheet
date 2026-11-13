using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TimeSheet.Configuration;
using TimeSheet.Domain.Dtos;
using Xunit;

namespace TimeSheet.Tests.Configuration
{
    public class ApiBehaviorServiceConfigTests
    {
        private static ActionContext ContextWithErrors()
        {
            var context = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            context.ModelState.AddModelError("Name", "The Name field is required.");
            context.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");
            return context;
        }

        [Fact]
        public void BuildInvalidModelStateResponse_ReturnsBadRequestWithMessageContainingAllErrors()
        {
            var result = ApiBehaviorServiceConfig.BuildInvalidModelStateResponse(ContextWithErrors());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var error = Assert.IsType<ErrorDto>(badRequest.Value);
            Assert.Contains("The Name field is required.", error.Message);
            Assert.Contains("The Email field is not a valid e-mail address.", error.Message);
        }

        [Fact]
        public void Configure_RegistersTheFactoryInApiBehaviorOptions()
        {
            var services = new ServiceCollection();
            services.AddControllers();
            ApiBehaviorServiceConfig.Configure(services);

            var options = services.BuildServiceProvider().GetRequiredService<IOptions<ApiBehaviorOptions>>().Value;
            var result = options.InvalidModelStateResponseFactory(ContextWithErrors());

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<ErrorDto>(badRequest.Value);
        }
    }
}
