using System.Text.Json.Serialization;

namespace Malte2.Model.Accounting
{

    public class RemissionOperationCheck
    {
        [JsonPropertyName("n")]
        public ulong CheckNumber { get; set; }

        [JsonPropertyName("dt")]
        public DateTime DateTime { get; set; }

        [JsonPropertyName("l")]
        public string Label { get; set; } = "";
        
        [JsonPropertyName("d")]
        public string Details { get; set; } = "";

        [JsonPropertyName("a")]
        public Amount Amount { get; set; }

        [JsonPropertyName("r")]
        public long? RemissionId { get; set; }
    }
}