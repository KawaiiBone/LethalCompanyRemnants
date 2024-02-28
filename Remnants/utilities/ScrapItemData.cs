using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Remnants.utilities
{
    internal class ScrapItemData
    {
        [JsonProperty("ScrapItemName")]
        public string ScrapItemName { get; set; }

        [JsonProperty("Isbanned")]
        public bool Isbanned { get; set; }
    }


}
