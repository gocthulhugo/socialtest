using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.SocialAlloy.Web.Social.Composites
{
    public class ReviewComment
    {
        public string ReviewId { get; set; }
        public string Text { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string ProductId { get; set; }
        public DateTime Created { get; set; }
    }
}