using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.SocialAlloy.Web.Social.ViewModels
{
    public class ReviewCommentsViewModel
    {
        public string ReviewId { get; set; }
        public string Text { get; set; }
        public int AuthorId { get; set; }
        public string ProductId { get; set; }
        public string AuthorName { get; set; }
        public string Created { get; set; }
    }
}