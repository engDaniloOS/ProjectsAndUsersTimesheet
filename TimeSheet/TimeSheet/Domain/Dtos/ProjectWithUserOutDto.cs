using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class ProjectWithUserOutDto
    {
        [JsonPropertyName("project_id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("users")]
        public List<UserOutDto> Users { get; set; }

        [JsonIgnore]
        public string Error { get; set; }
    }
}
