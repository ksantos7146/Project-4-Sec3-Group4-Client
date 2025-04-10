using System;

namespace Project4_Client.Models
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Bio { get; set; } = string.Empty;
        public Image[] Images { get; set; } = Array.Empty<Image>();
        public int GenderId { get; set; }
        public int StateId { get; set; }
    }
} 