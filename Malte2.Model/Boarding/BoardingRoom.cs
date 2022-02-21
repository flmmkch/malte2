using System.Text.Json.Serialization;

namespace Malte2.Model.Boarding
{

    /// <summary>
    /// Pensionnaire
    /// </summary>
    public class BoardingRoom : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; } = null;

        [JsonPropertyName("l")]
        public string Label { get; set; } = "";
    }

}