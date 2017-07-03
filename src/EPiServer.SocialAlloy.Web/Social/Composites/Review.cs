using System.Collections.Generic;
using EPiServer.Social.Comments.Core;

namespace EPiServer.SocialAlloy.Web.Social.Composites
{
    public class Review
    {
        public string ReviewId { get; set; }
        public string Title { get; set; }

        public string Nickname { get; set; }

        public string Location { get; set; }

        public string ProductId { get; set; }
        public string ProductName { get; set; }

        public ReviewRating Rating { get; set; }
        public List<ReviewSecondaryRating> SecondaryRatings { get; set; }
        public List<ReviewComment> Comments { get; set; }
    }
}