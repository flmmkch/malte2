using System.Text.Json.Serialization;

namespace Malte2.Model.Boarding
{

    public struct BoarderListItemResponse
    {
        [JsonPropertyName("b")]
        public long BoarderId { get; set; }

        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("r")]
        public string? RoomName { get; set; }

    }
}