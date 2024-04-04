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


}
