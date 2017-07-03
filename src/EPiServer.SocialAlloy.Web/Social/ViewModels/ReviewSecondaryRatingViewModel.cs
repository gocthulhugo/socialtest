using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.SocialAlloy.Web.Social.ViewModels
{
    public class ReviewSecondaryRatingViewModel
    {
        public string Label { get; set; }
        public string RatingValue { get; set; }
        public List<SelectListItem> PossibleValues { get; set; }

        public override string ToString()
        {
            return RatingValue;
        }
    }
}