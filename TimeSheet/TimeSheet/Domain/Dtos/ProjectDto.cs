using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class ProjectDto
    {
        [JsonPropertyName("user_id")]
        public List<int> UsersIds { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}
