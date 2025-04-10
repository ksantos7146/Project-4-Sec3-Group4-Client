using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Project4_Client.Models
{
    public class User
    {
        [JsonProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonProperty("bio")]
        public string Bio { get; set; } = string.Empty;

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; } = new List<Image>();

        [JsonIgnore]
        public BitmapImage? ProfileImage { get; set; }
    }

    public class Image
    {
        [JsonProperty("imageId")]
        public int ImageId { get; set; }

        [JsonProperty("imageData")]
        public string ImageData { get; set; } = string.Empty;
    }
} 