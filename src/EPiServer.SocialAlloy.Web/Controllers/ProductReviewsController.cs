using System.Web.Mvc;
using EPiServer.ServiceLocation;
using EPiServer.SocialAlloy.Web.Social.Services;
using EPiServer.SocialAlloy.Web.Social.ViewModels;

namespace EPiServer.SocialAlloy.Web.Controllers
{
    public class ProductReviewsController : Controller
    {
        private readonly IReviewService _reviewService;

        public ProductReviewsController()
        {
            _reviewService = ServiceLocator.Current.GetInstance<IReviewService>();
        }

        public ActionResult Index(string productId)
        {
            var reviews = _reviewService.Get(productId);
            return View("~/Views/Social/Reviews/Reviews.cshtml", reviews);
        }

        [HttpPost]
        public ActionResult Submit(ReviewSubmissionViewModel submissionViewModel)
        {
            // Add the review
            _reviewService.Add(submissionViewModel);
            // Return to the product list of reviews
            //return Index(submissionViewModel.ProductId);
            return RedirectToAction("Index", new { productId = submissionViewModel.ProductId });
        }

        [HttpPost]
        public ActionResult Comment(ReviewCommentsViewModel model)
        {
            // submit comment
            _reviewService.Comment(model);
            // refresh page
            return RedirectToAction("Index", new {productId = model.ProductId});
        }
    }
}