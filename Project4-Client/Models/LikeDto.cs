using System;

namespace Project4_Client.Models
{
    public class LikeDto
    {
        public int? LikeId { get; set; }
        public string? LikerId { get; set; } = string.Empty;
        public string LikedId { get; set; } = string.Empty;
        public DateTime? LikedAt { get; set; }
        public bool likedBack { get; set; } = false;
    }

    public class LikeResponseDto
    {
        public LikeDto Like { get; set; } = new LikeDto();
        public User NextUser { get; set; }
    }
} 