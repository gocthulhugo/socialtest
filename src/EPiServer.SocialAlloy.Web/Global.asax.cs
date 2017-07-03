using System.Web.Mvc;
using System.Web.Routing;

namespace EPiServer.SocialAlloy.Web
{
    public class EPiServerApplication : EPiServer.Global
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //Tip: Want to call the EPiServer API on startup? Add an initialization module instead (Add -> New Item.. -> EPiServer -> Initialization Module)
        }

        protected override void RegisterRoutes(RouteCollection routes)
        {
            base.RegisterRoutes(routes);

            routes.MapRoute(
                "Moderation",
                "moderation/{action}/{workflowItemId}",
                new
                {
                    controller = "Moderation",
                    action = "Index",
                    workflowItemId = UrlParameter.Optional
                }
            );

            routes.MapRoute(
                "ProductReviews",
                "reviews/{productId}",
                new
                {
                    controller = "ProductReviews",
                    action = "Index",
                    productId = UrlParameter.Optional

                }
            );

            routes.MapRoute(
                "ReviewCommentRoute",
                "review/comment/{id}",
                new
                {
                    controller = "ProductReviews",
                    action = "Comment",
                    id = UrlParameter.Optional
                }
            );

            routes.MapRoute(
                "ProductReviewsSubmission",
                "reviews/submit/{productId}",
                new {
                    controller = "ProductReviews",
                    action = "Submit",
                    productId = UrlParameter.Optional

                }
            );
        }
    }
}