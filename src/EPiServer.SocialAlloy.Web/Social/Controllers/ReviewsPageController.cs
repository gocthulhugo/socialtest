using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.ServiceLocation;
using EPiServer.SocialAlloy.Web.Models.ViewModels;
using EPiServer.SocialAlloy.Web.Social.Pages;
using EPiServer.SocialAlloy.Web.Social.Services;
using EPiServer.SocialAlloy.Web.Social.ViewModels;
using EPiServer.Web.Mvc;
using EPiServer.Web.PageExtensions;

namespace EPiServer.SocialAlloy.Web.Social.Controllers
{
    public class ReviewsPageController : PageController<ReviewsPage>
    {
        private readonly IReviewService _reviewService;

        public ReviewsPageController()
        {
            _reviewService = ServiceLocator.Current.GetInstance<IReviewService>();
        }

        public ActionResult Index(ReviewsPage currentPage)
        {
            var form = GetReviewForm(currentPage.ProductId, currentPage.ProductName);
            var model = new PageViewModel<ReviewsPage>(currentPage) {CurrentPage = {ReviewForm = form}};
            return View("~/Views/Social/Reviews/ReviewForm.cshtml", model);
        }
        
        private static ReviewSubmissionViewModel GetReviewForm(string productId, string productName)
        {
            return new ReviewSubmissionViewModel
            {
                ProductId = productId,
                ProductName = productName
            };
        }
    }
}