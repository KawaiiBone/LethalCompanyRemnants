using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
