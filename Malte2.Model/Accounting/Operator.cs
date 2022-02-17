using System.Text.Json.Serialization;

namespace Malte2.Model.Accounting
{

    public class Operator : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("e")]
        public bool Enabled { get; set; } = true;

        public Operator(string name)
        {
            Name = name;
        }

        public bool ShouldSerializeEnabled()
        {
            return !Enabled;
        }
    }

}