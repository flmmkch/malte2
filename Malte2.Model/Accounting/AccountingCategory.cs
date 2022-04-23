using System.Text.Json.Serialization;

namespace Malte2.Model.Accounting
{

    /// <summary>
    /// Catégorie d'opération comptable
    /// </summary>
    public class AccountingCategory : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; } = null;

        [JsonPropertyName("l")]
        public string Label { get; set; } = "";

        [JsonPropertyName("ae")]
        public long? AccountingEntryId { get; set; }
    }

}