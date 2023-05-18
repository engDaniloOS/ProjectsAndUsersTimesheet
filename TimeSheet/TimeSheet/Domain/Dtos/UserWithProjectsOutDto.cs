using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class UserWithProjectsOutDto: UserOutDto
    {
        [JsonPropertyName("projects")]
        public List<ProjectOutDto> Projects { get; set; }
    }
}
