using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeSheet.Domain.Dtos;
using TimeSheet.Domain.UseCases;

namespace TimeSheet.Connectors.Controllers
{
    [Route("api/v1/times")]
    [ApiController]
    [Authorize]
    public class TimesController : ControllerBase
    {
        private readonly IWorkTimeService _service;

        public TimesController(IWorkTimeService service) => _service = service;

        [HttpPost]
        public async Task<IActionResult> SaveNewTime([FromBody] WorkTimeDto workTimeDto)
        {
            var responseDto = await _service.Save(workTimeDto);

            if (!string.IsNullOrWhiteSpace(responseDto.Error))
                return BadRequest(new ErrorDto { Message = responseDto.Error });

            if (responseDto.Id == 0)
                return NotFound();

            return Ok(responseDto);
        }

        [HttpGet]
        [Route("{project_id}")]
        public async Task<IActionResult> GetWorkTimesByProjectId([FromRoute(Name = "project_id")] int projectId)
        {
            WorkTimesOutDto times = await _service.GetWorkTimesByProjectId(projectId);

            if (!string.IsNullOrWhiteSpace(times.Error))
                return BadRequest(new ErrorDto { Message = times.Error });

            if (times.WorkTimes.Count == 0)
                return NotFound();

            return Ok(times);
        }

        [HttpPut]
        [Route("{time_id}")]
        public async Task<IActionResult> EditWorkTimeById([FromRoute(Name = "time_id")] long workTimeId, [FromBody] WorkTimeDto workTimeDto)
        {
            WorkTimeOutDto worktime = await _service.EditWorkTime(workTimeId, workTimeDto);

            if (!string.IsNullOrWhiteSpace(worktime.Error))
                return BadRequest(new ErrorDto { Message = worktime.Error });

            if (worktime.Id == 0)
                return NotFound();

            return Ok(worktime);
        }
    }
}
