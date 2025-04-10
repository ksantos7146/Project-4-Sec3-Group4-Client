using System;

namespace Project4_Client.Models
{
    public class MatchDto
    {
        public int? MatchId { get; set; }
        public string User1Id { get; set; } = string.Empty;
        public string User2Id { get; set; } = string.Empty;
        public DateTime MatchedAt { get; set; }
    }
} 