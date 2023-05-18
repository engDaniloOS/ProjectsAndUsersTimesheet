using System.Text.Json.Serialization;

namespace TimeSheet.Domain.Dtos
{
    public class ProjectOutDto
    {
        [JsonPropertyName("project_id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonIgnore]
        public string Error { get; set; }
    }
}
