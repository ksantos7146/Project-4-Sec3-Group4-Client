using System;

namespace Project4_Client.Models
{
    public class LikeDto
    {
        public string LikedId { get; set; }
        public bool likedBack { get; set; }
    }

    public class LikeResponseDto
    {
        public LikeDto Like { get; set; }
        public User NextUser { get; set; }
    }
} 