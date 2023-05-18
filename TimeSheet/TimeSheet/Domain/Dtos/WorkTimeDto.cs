using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class WorkTimeDto
    {
        [Required]
        [JsonPropertyName("project_id")]
        public int ProjectId { get; set; }

        [Required]
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [Required]
        [JsonPropertyName("started_at")]
        public DateTime Start { get; set; }

        [Required]
        [JsonPropertyName("ended_at")]
        public DateTime End { get; set; }
    }
}
