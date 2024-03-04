using Newtonsoft.Json;

namespace Remnants.utilities
{
    internal class RemnantData
    {
        [JsonProperty("RemnantItemName")]
        public string RemnantItemName { get; set; }

        [JsonProperty("ShouldSpawn")]
        public bool ShouldSpawn { get; set; }
    }


}
