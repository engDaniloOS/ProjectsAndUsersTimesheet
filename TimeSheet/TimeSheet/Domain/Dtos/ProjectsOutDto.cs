using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class ProjectsOutDto
    {
        [JsonPropertyName("projects")]
        public List<ProjectWithUserOutDto> Projects { get; set; }

        [JsonIgnore]
        public string Error { get; set; }
    }
}
