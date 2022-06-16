namespace AceSmokeShop.Models
{
    
    public class ImageAPIResponse
    {
        public Data data { get; set; }
        public object ContentEncodeing { get; set; }

        public object ContentType { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public string title { get; set; }
        public string url_viewer { get; set; }
        public string url { get; set; }
        public string display_url { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string size { get; set; }
        public string time { get; set; }
        public string expiration { get; set; }
        public Image image { get; set; }
        public Thumb thumb { get; set; }
        public Medium medium { get; set; }
        public string delete_url { get; set; }
    }

    public class Image
    {
        public string filename { get; set; }
        public string name { get; set; }
        public string mime { get; set; }
        public string extension { get; set; }
        public string url { get; set; }
    }

    public class Medium
    {
        public string filename { get; set; }
        public string name { get; set; }
        public string mime { get; set; }
        public string extension { get; set; }
        public string url { get; set; }
    }
    public class Root
    {
        public Data data { get; set; }
        public bool success { get; set; }
        public int status { get; set; }
    }
  
    public class Thumb
    {
        public string filename { get; set; }
        public string name { get; set; }
        public string mime { get; set; }
        public string extension { get; set; }
        public string url { get; set; }
    }
}
