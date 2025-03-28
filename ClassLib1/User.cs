namespace ClassLib1
{
    public class User
    {
        public int userId { get; set; }
        public string username { get; set; }
        public string bio { get; set; }
        public int genderId { get; set; }
        public int stateId { get; set; }
        public int age { get; set; }
        public Image[] images { get; set; }
    }

    public class Image
    {
        public int imageId { get; set; }
        public string imageData { get; set; }
        public DateTime uploadedAt { get; set; }
        public int userId { get; set; }
    }
}
