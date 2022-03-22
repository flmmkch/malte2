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
        [JsonPropertyName("pm")]
        public PaymentMethod PaymentMethod { get; set; }
        [JsonPropertyName("b")]
        public long AccountBookId { get; set; }
        /// <summary>Numéro de chèque optionnel (uniquement pour les paiements par chèque)</summary>
        [JsonPropertyName("pi")]
        public string PaymentMethodInfo { get; set; } = "";
        [JsonPropertyName("l")]
        public string Label { get; set; } = "";
        [JsonPropertyName("bd")]
        public long? BoarderId { get; set; }
    }

}