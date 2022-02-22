using Malte2.Model.Accounting;
using System.Text.Json.Serialization;

namespace Malte2.Model.Boarding
{

    /// <summary>
    /// Pensionnaire
    /// </summary>
    public class Boarder : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; } = null;

        [JsonPropertyName("n")]
        public string Name { get; set; } = "";

        [JsonPropertyName("na")]
        public string Nationality { get; set; } = "";

        [JsonPropertyName("bd")]
        public DateTime? BirthDate { get; set; }

        [JsonPropertyName("bp")]
        public string? BirthPlace { get; set; }

        [JsonPropertyName("p")]
        public string PhoneNumber { get; set; } = "";

        [JsonPropertyName("m")]
        public string Notes { get; set; } = "";

        [JsonPropertyName("a")]
        public Amount TotalAmountDeposited { get; set; } = new Amount();
    }

}