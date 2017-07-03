using System;
using System.Collections.Generic;

namespace EPiServer.SocialAlloy.Web.Social.ViewModels
{
    public class ReviewViewModel
    {
        public string ReviewId { get; set; }
        public string Title { get; set; }

        public string Body { get; set; }

        public string Nickname { get; set; }

        public string Location { get; set; }

        public int Rating { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }

        public DateTime AddedOn { get; set; }
        public List<ReviewCommentsViewModel> Comments { get; set; }
        public List<ReviewSecondaryRatingViewModel> SecondaryRatings { get; set; }
    }
}