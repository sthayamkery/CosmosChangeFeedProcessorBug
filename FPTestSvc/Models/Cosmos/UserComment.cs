using Newtonsoft.Json;
using System;

namespace WK.FPTest.Models.Cosmos
{
    public class UserComment
    {

        [JsonProperty("userId")]
        public int UserId { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("sourceid")]
        public Guid SourceId { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("firmId")]
        public int FirmId { get; set; }
        [JsonProperty("host")]
        public string Host { get; set; }
        [JsonProperty("consumer")]
        public string Consumer { get; set; }

    }
}
