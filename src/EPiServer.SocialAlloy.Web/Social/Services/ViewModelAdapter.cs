using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Social.Comments.Core;
using EPiServer.Social.Common;
using EPiServer.Social.Ratings.Core;
using EPiServer.SocialAlloy.Web.Social.Composites;
using EPiServer.SocialAlloy.Web.Social.ViewModels;

namespace EPiServer.SocialAlloy.Web.Social.Services
{
    internal static class ViewModelAdapter
    {
        public static ReviewStatisticsViewModel Adapt(RatingStatistics statistics)
        {
            var viewModel = new ReviewStatisticsViewModel();

            if (statistics != null)
            {
                viewModel.OverallRating = Convert.ToDouble(statistics.Sum) / Convert.ToDouble(statistics.TotalCount);
                viewModel.TotalRatings = statistics.TotalCount;
            }

            return viewModel;
        }
        
        public static IEnumerable<ReviewViewModel> Adapt(IEnumerable<Composite<Comment, Review>> reviews)
        {
            return reviews.Select(Adapt);
        }

        private static ReviewViewModel Adapt(Composite<Comment, Review> review)
        {
            return new ReviewViewModel
            {
                ReviewId = review.Data.Id.ToString(),
                AddedOn = review.Data.Created,
                Body = review.Data.Body,
                Location = review.Extension.Location,
                Nickname = review.Extension.Nickname,
                Rating = review.Extension.Rating.Value,
                Title = review.Extension.Title,
                ProductId = review.Extension.ProductId,
                ProductName = review.Extension.ProductName,
                Comments = Adapt(review.Extension.Comments),
                SecondaryRatings = Adapt(review.Extension.SecondaryRatings)
            };
        }

        public static List<ReviewCommentsViewModel> Adapt(List<ReviewComment> comments)
        {
            var commentsViewModel = new List<ReviewCommentsViewModel>();
            if(comments == null)
                return commentsViewModel;
            foreach (var comment in comments)
            {
                var viewModel = new ReviewCommentsViewModel
                {
                    ProductId = comment.ProductId,
                    ReviewId = comment.ReviewId,
                    Created = comment.Created.ToString("MMMM dd, yyyy"),
                    AuthorId = comment.AuthorId,
                    AuthorName = comment.AuthorName,
                    Text = comment.Text
                };
                commentsViewModel.Add(viewModel);
            }
            return commentsViewModel;
        }

        public static List<ReviewSecondaryRatingViewModel> Adapt(List<ReviewSecondaryRating> ratings)
        {
            var viewModel = new List<ReviewSecondaryRatingViewModel>();
            if (ratings == null)
                return viewModel;
            foreach (var rating in ratings)
            {
                var rate = new ReviewSecondaryRatingViewModel
                {
                    Label = rating.Label,
                    RatingValue = rating.RatingValue
                };
                viewModel.Add(rate);
            }
            return viewModel;
        }
    }
}
