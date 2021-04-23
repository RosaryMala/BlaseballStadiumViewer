using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QuickType
{

    [Serializable]
    public partial class StadiumData
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("hype")]
        public int Hype { get; set; }

        [JsonProperty("mods")]
        public List<string> Mods { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("mainColor")]
        public string MainColor { get; set; }

        [JsonProperty("secondaryColor")]
        public string SecondaryColor { get; set; }

        [JsonProperty("tertiaryColor")]
        public string TertiaryColor { get; set; }

        [JsonProperty("mysticism")]
        public float Mysticism { get; set; }

        [JsonProperty("viscosity")]
        public float Viscosity { get; set; }

        [JsonProperty("elongation")]
        public float Elongation { get; set; }

        [JsonProperty("filthiness")]
        public float Filthiness { get; set; }

        [JsonProperty("obtuseness")]
        public float Obtuseness { get; set; }

        [JsonProperty("forwardness")]
        public float Forwardness { get; set; }

        [JsonProperty("grandiosity")]
        public float Grandiosity { get; set; }

        [JsonProperty("ominousness")]
        public float Ominousness { get; set; }

        [JsonProperty("fortification")]
        public float Fortification { get; set; }

        [JsonProperty("inconvenience")]
        public float Inconvenience { get; set; }

        [JsonProperty("luxuriousness")]
        public float Luxuriousness { get; set; }
    }

    [Serializable]
    public partial class Stadium
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("lastUpdate")]
        public DateTimeOffset LastUpdate { get; set; }

        [JsonProperty("data")]
        public StadiumData Data { get; set; }
    }

    [Serializable]
    public partial class SibrStadiumList
    {
        [JsonProperty("data")]
        public List<Stadium> Data { get; set; }
    }
}
