using System.Text.Json.Serialization;

namespace Malte2.Model.Boarding
{

    public struct BoarderListItemResponse : IHasObjectId
    {
        [JsonPropertyName("b")]
        public long BoarderId { get; set; }

        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("r")]
        public string? RoomName { get; set; }

        public long? Id { get => BoarderId; set => BoarderId = value.GetValueOrDefault(-1); }

    }
}