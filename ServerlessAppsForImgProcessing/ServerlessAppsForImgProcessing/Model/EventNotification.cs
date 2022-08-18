namespace ServerlessAppsForImgProcessing.Model
{
    public class EventNotification
    {
        public string Version { get; set; }
        public string Id { get; set; }
        public Detail Detail { get; set; }
    }

    public class Detail
    {
        public Bucket Bucket { get; set; }
        public Object Object { get; set; }
    }

    public class Bucket
    {
        public string Name { get; set; }
    }

    public class Object
    {
        public string Key { get; set; }
    }
}
