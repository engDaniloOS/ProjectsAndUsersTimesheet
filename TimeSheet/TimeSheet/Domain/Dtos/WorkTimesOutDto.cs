using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class WorkTimesOutDto
    {
        [JsonPropertyName("time")]
        public List<WorkTimeOutDto> WorkTimes { get; set; }

        [JsonIgnore]
        public string Error { get; set; }
    }
}
