using System;

namespace Project4_Client.Models
{
    public class PreferenceDto
    {
        public int? PreferenceId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public int? GenderId { get; set; }
    }
} 