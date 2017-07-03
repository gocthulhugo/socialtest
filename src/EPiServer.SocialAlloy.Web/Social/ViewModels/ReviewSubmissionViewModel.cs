using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EPiServer.SocialAlloy.Web.Social.ViewModels
{
    public class ReviewSubmissionViewModel
    {
        public ReviewSubmissionViewModel()
        {
            Awesomeness = new ReviewSecondaryRatingViewModel
            {
                Label = "Awesomeness",
                RatingValue = "1",
                PossibleValues = GetStars()
            };
            OverallValue = new ReviewSecondaryRatingViewModel
            {
                Label = "Overall value",
                RatingValue = "1",
                PossibleValues = GetStars()
            };
            RandomQuestion = new ReviewSecondaryRatingViewModel
            {
                Label = "Random question",
                RatingValue = "1",
                PossibleValues = GetStars()
            };
        }

        public ReviewSubmissionViewModel(string productCode)
        {
            this.ProductId = productCode;
            Awesomeness = new ReviewSecondaryRatingViewModel
            {
                Label = "Awesomeness", RatingValue = "1", PossibleValues = GetStars()
            };
            OverallValue = new ReviewSecondaryRatingViewModel
            {
                Label = "Overall value", RatingValue = "1" , PossibleValues = GetStars()
            };
            RandomQuestion = new ReviewSecondaryRatingViewModel
            {   
                Label = "Random question", RatingValue = "1", PossibleValues = GetStars()
            };
        }

        [Required]
        public string ProductId { get; set; }

        public string ProductName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a title for your review.")]
        public string Title { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please add a description to your review.")]
        public string Body { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide your nickname.")]
        public string Nickname { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide your location.")]
        public string Location { get; set; }

        [Range(1, 5, ErrorMessage = "Please provide a rating from 1 to 5.")]
        public int Rating { get; set; }

        public ReviewSecondaryRatingViewModel Awesomeness { get; set; }
        public ReviewSecondaryRatingViewModel OverallValue { get; set; }
        public ReviewSecondaryRatingViewModel RandomQuestion { get; set; }

        public List<SelectListItem> GetStars(int scale = 5)
        {
            var stars = new List<SelectListItem>();
            for (var i = scale; i >= 1; i--)
            {
                stars.Add(new SelectListItem { Value = i.ToString(), Text = $"{i} Stars" });
            }
            return stars;
        }
    }
}