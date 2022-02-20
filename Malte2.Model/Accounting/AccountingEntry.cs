using System.Text.Json.Serialization;

namespace Malte2.Model.Accounting
{

    /// <summary>
    /// Imputation comptable
    /// </summary>
    public class AccountingEntry : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; } = null;

        [JsonPropertyName("l")]
        public string Label { get; set; } = "";

        [JsonPropertyName("b")]
        public bool HasBoarder { get; set; } = false;

        [JsonPropertyName("t")]
        public AccountingEntryType EntryType { get; set; } = AccountingEntryType.Expense;
    }

}