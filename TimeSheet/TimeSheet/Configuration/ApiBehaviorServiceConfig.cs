using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using TimeSheet.Domain.Dtos;

namespace TimeSheet.Configuration
{
    public static class ApiBehaviorServiceConfig
    {
        public static void Configure(IServiceCollection services)
            => services.Configure<ApiBehaviorOptions>(options =>
                options.InvalidModelStateResponseFactory = BuildInvalidModelStateResponse);

        /// <summary>
        /// Converte erros de validação de modelo (DataAnnotations) no mesmo formato de erro
        /// usado pelos controllers: 400 com <see cref="ErrorDto"/>.
        /// </summary>
        public static IActionResult BuildInvalidModelStateResponse(ActionContext context)
        {
            var messages = context.ModelState.Values
                                  .SelectMany(entry => entry.Errors)
                                  .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                                       ? "The request body is invalid."
                                                       : error.ErrorMessage)
                                  .Distinct();

            return new BadRequestObjectResult(new ErrorDto { Message = string.Join(" ", messages) });
        }
    }
}
