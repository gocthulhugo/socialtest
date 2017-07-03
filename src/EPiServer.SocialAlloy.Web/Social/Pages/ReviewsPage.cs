using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SocialAlloy.Web.Models.Pages;
using EPiServer.SocialAlloy.Web.Social.Blocks;
using EPiServer.SocialAlloy.Web.Social.Composites;
using EPiServer.SocialAlloy.Web.Social.ViewModels;

namespace EPiServer.SocialAlloy.Web.Social.Pages
{
    [ContentType(
        DisplayName = "Reviews Page",
        GUID = "0d8755d4-5e52-4b18-a4f8-fb56a72d65f0",
        Description = "Test review page")]
    public class ReviewsPage : StandardPage
    {
        public ReviewsPage()
        {
            ProductReviews = new ReviewsViewModel(ProductId, ProductName);
            
            ReviewForm = new ReviewSubmissionViewModel
            {
                ProductId = ProductId,
                ProductName = ProductName
            };
        }

        /// <summary>
        /// The feed section of the photos page. Local feed block will display feed items.
        /// </summary>
        [Display(
            Name = "Feed Block",
            Description = "The feed section of the reviews page. Local feed block will display feed items for the pages to which a user has subscribed.",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual FeedBlock Feed { get; set; }

        public virtual string ProductId { get; set; }
        public virtual string ProductName { get; set; }

        [Ignore]
        public ReviewSubmissionViewModel ReviewForm { get; set; }

        [Ignore]
        public ReviewsViewModel ProductReviews { get; set; }
    }
}