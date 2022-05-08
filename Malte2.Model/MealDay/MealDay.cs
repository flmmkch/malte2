using System.Text.Json.Serialization;

namespace Malte2.Model.MealDay
{

    /// <summary>
    /// Opération comptable
    /// </summary>
    public class MealDay : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; } = null;
        [JsonPropertyName("dt")]
        public DateTime MealDateTime { get; set; }
        [JsonPropertyName("nb")]
        public int BoarderCount { get; set; } = 0;
        [JsonPropertyName("np")]
        public int PatronCount { get; set; } = 0;
        [JsonPropertyName("no")]
        public int OtherCount { get; set; } = 0;
        [JsonPropertyName("nc")]
        public int CatererCount { get; set; } = 0;
    }

}