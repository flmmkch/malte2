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
        [JsonPropertyName("op")]
        public long OperatorId { get; set; }
        [JsonPropertyName("dt")]
        public DateTime OperationDateTime { get; set; }
        /// <summary>Imputation comptable</summary>
        [JsonPropertyName("ae")]
        public long AccountingEntryId { get; set; }
        [JsonPropertyName("pm")]
        public PaymentMethod PaymentMethod { get; set; }
        /// <summary>Numéro de chèque optionnel (uniquement pour les paiements par chèque)</summary>
        [JsonPropertyName("pm")]
        public string? PaymentCheckNumber { get; set; }
        [JsonPropertyName("l")]
        public string Label { get; set; } = "";
        [JsonPropertyName("b")]
        public long? BoarderId { get; set; }

        public Operation(long operatorId, DateTime operationDateTime, long accountingEntryId, PaymentMethod paymentMethod)
        {
            (OperatorId, OperationDateTime, AccountingEntryId, PaymentMethod) = (operatorId, operationDateTime, accountingEntryId, paymentMethod);
        }
    }

}