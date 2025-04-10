using System;

namespace Project4_Client.Models
{
    public class User
    {
        public string userId { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string bio { get; set; }
        public int age { get; set; }
        public Image[] images { get; set; }
    }

    public class Image
    {
        public int imageId { get; set; }
        public string imageData { get; set; }
    }
} 