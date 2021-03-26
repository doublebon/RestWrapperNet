using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Example.support
{
    class ObjToJson
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("job")]
        public string Job { get; set; }
    }
}
