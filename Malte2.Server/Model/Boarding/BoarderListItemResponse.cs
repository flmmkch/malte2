using System.Text.Json.Serialization;
using Malte2.Model.Accounting;

namespace Malte2.Model.Boarding
{

    public class BoarderListItemResponse : IHasObjectId
    {
        [JsonPropertyName("b")]
        public long BoarderId { get; set; }

        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("r")]
        public string? RoomName { get; set; }

        [JsonPropertyName("a")]
        public Amount? Balance { get; set; }

        public long? Id { get => BoarderId; set => BoarderId = value.GetValueOrDefault(-1); }

    }
}