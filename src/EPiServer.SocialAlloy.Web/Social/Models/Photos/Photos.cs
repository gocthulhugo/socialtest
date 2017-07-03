using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.SocialAlloy.Web.Social.Models.Photos
{
    public class Photos
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public List<Photo> CustomerPhotos { get; set; }
        public int Total => CustomerPhotos.Count();
        public string PageId { get; set; }
    }

    public class Photo
    {
        public int PhotoId { get; set; }
        public string Uri { get; set; }
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime Created { get; set; }
        public Status Status { get; set; }
        public List<PageComment> Comments { get; set; } 
    }

    public enum Status
    {
        Pending,
        Approved,
        Rejected
    }
}