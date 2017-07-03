using EPiServer.SocialAlloy.Web.Social.ViewModels;

namespace EPiServer.SocialAlloy.Web.Social.Services
{
    public interface IReviewService
    {
        void Add(ReviewSubmissionViewModel review);
        ReviewsViewModel Get(string productCode);

        void Comment(ReviewCommentsViewModel comment);
    }
}
