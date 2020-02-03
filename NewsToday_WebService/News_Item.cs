using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsToday_WebService
{
    /*
     * This is list of objects class
     */
    public class News_Item
    {
        public int NewsID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Desp { get; set; }
        public string UrlTOIMG { get; set; }
        public string Content { get; set; }
        public DateTime PublishedAt { get; set; }
        public int NiD { get; set; }
        public string URL { get; set; }

        public News_Item(int newsID, string title, string author, string desp, string urlTOIMG, string content, DateTime publishedAt, int niD, string uRL)
        {
            NewsID = newsID;
            Title = title;
            Author = author;
            Desp = desp;
            UrlTOIMG = urlTOIMG;
            Content = content;
            PublishedAt = publishedAt;
            NiD = niD;
            URL = uRL;
        }
    }
}