using System;
using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class WorkTimeOutDto
    {
        [JsonPropertyName("time_id")]
        public int Id { get; set; }

        [JsonPropertyName("project")]
        public ProjectOutDto Project { get; set; }

        [JsonPropertyName("user")]
        public UserOutDto User { get; set; }

        [JsonPropertyName("started_at")]
        public DateTime Start { get; set; }

        [JsonPropertyName("ended_at")]
        public DateTime End { get; set; }

        [JsonIgnore]
        public string Error { get; set; }
    }
}
