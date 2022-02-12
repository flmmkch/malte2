using System.Text.Json.Serialization;

namespace Malte2.Model.Accounting
{

    public class Operator : IHasObjectId
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("n")]
        public string Name { get; set; }

        public Operator(string name)
        {
            Name = name;
        }
    }

}