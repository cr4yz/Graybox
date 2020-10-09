using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Graybox
{
    public class gb_MapInfo : gb_Object
    {
        [JsonProperty]
        public string Name;
        [JsonProperty]
        public List<gb_Object> Objects = new List<gb_Object>();
    }
}

