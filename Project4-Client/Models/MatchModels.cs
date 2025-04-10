using System;
using Newtonsoft.Json;

namespace Project4_Client.Models
{
    public class MatchDto
    {
        [JsonProperty("matchId")]
        public int? MatchId { get; set; }

        [JsonProperty("user1Id")]
        public string User1Id { get; set; } = string.Empty;

        [JsonProperty("user2Id")]
        public string User2Id { get; set; } = string.Empty;

        [JsonProperty("matchedAt")]
        public DateTime MatchedAt { get; set; }

        [JsonProperty("user1")]
        public User? User1 { get; set; }

        [JsonProperty("user2")]
        public User? User2 { get; set; }
    }
} 