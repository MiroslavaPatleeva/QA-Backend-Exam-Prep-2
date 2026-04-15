using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoodyExamPrep2.Models
{
    internal class FoodDTO
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("url")]
        public string? Url { get; set; }
    }
}
