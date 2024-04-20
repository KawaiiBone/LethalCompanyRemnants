using Newtonsoft.Json;

namespace Remnants.utilities
{
    public class RemnantData
    {
        [JsonProperty("RemnantItemName")]
        public string RemnantItemName { get; set; }

        [JsonProperty("ShouldSpawn")]
        public bool ShouldSpawn { get; set; }
    }

    public class SuitData
    {
        [JsonProperty("SuitName")]
        public string SuitName { get; set; }

        [JsonProperty("UseSuit")]
        public bool UseSuit { get; set; }
    }
}
