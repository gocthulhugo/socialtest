using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.SocialAlloy.Web.Social.Models.Comments
{
    public class CommentsExtension
    {
        public bool IsImage { get; set; }

        public bool IsSubComment { get; set; }

        // Photo this comment is associated with!!
        public int PhotoId { get; set; }
        public string ProductId { get; set; }
    }
}