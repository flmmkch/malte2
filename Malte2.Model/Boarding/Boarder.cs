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
        public string Name { get; set; }

        public Boarder(string name)
        {
            Name = name;
        }
    }

}