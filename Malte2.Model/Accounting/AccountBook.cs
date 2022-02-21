using System.Text.Json.Serialization;

namespace Malte2.Model.Accounting
{

    /// <summary>
    /// Livre comptable
    /// </summary>
    public class AccountBook : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; } = null;

        [JsonPropertyName("l")]
        public string Label { get; set; } = "";
    }

}