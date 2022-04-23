using System;
using System.Text.Json.Serialization;

namespace Malte2.Model.Accounting
{

    /// <summary>
    /// Opération comptable
    /// </summary>
    public class Operation : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; } = null;
        [JsonPropertyName("a")]
        public Amount Amount { get; set; } = new Amount();
        [JsonPropertyName("op")]
        public long OperatorId { get; set; }
        [JsonPropertyName("dt")]
        public DateTime OperationDateTime { get; set; }
        /// <summary>Imputation comptable</summary>
        [JsonPropertyName("ae")]
        public long AccountingEntryId { get; set; }
        [JsonPropertyName("ac")]
        public long? AccountingCategoryId { get; set; }
        [JsonPropertyName("pm")]
        public PaymentMethod PaymentMethod { get; set; }
        [JsonPropertyName("b")]
        public long AccountBookId { get; set; }
        [JsonPropertyName("l")]
        public string Label { get; set; } = "";
        [JsonPropertyName("bd")]
        public long? BoarderId { get; set; }
        [JsonPropertyName("pkn")]
        public long? CheckNumber { get; set; }
        [JsonPropertyName("ptn")]
        public long? TransferNumber { get; set; }
        [JsonPropertyName("ctn")]
        public long? CardTicketNumber { get; set; }
    }

}