using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EPiServer.ServiceLocation;
using EPiServer.Social.Comments.Core;
using EPiServer.Social.Common;
using EPiServer.Social.Ratings.Core;
using EPiServer.SocialAlloy.Web.Social.Composites;
using EPiServer.SocialAlloy.Web.Social.ViewModels;

namespace EPiServer.SocialAlloy.Web.Social.Services
{
    /// <summary>
    /// The ReviewService manages reviews contributed for products by leveraging
    /// Episerver Social's comments and ratings features.
    /// </summary>
    [ServiceConfiguration(typeof(IReviewService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ReviewService : IReviewService
    {
        private readonly ICommentService _commentService;
        private readonly IRatingService _ratingService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commentService">Episerver Social comment service</param>
        /// <param name="ratingService">Episerver Social rating service</param>
        public ReviewService(ICommentService commentService, IRatingService ratingService)
        {
            this._commentService = commentService;
            this._ratingService = ratingService;
        }

        /// <summary>
        /// Adds a review for the identified product.
        /// </summary>
        /// <param name="review">Review to be added</param>
        public void Add(ReviewSubmissionViewModel review)
        {
            // Instantiate a reference for the product

            var product = CreateProductReference(review.ProductId);

            // Instantiate a reference for the contributor

            var contributor = CreateContributorReference(review.Nickname);

            // Add the contributor's rating for the product

            var submittedRating = new Rating(contributor, product, new RatingValue(review.Rating));
            var storedRating = this._ratingService.Add(submittedRating);

            // Compose a comment representing the review

            var comment = new Comment(product, contributor, review.Body, true);

            var extension = new Review
            {
                ReviewId = comment.Id.ToString(),
                Title = review.Title,
                Location = review.Location,
                Nickname = review.Nickname,
                ProductId = review.ProductId,
                ProductName = review.ProductName,
                Rating = new ReviewRating
                {
                    Value = review.Rating,
                    Reference = storedRating.Id.Id
                },
                SecondaryRatings = SetSecondaryRatings(review)
            };

            // Add the composite comment for the product

            this._commentService.Add(comment, extension);
        }

        /// <summary>
        /// Gets the reviews that have been submitted for the identified product.
        /// </summary>
        /// <param name="productCode">Content code identifying the product</param>
        /// <returns>Reviews that have been submitted for the product</returns>
        public ReviewsViewModel Get(string productCode)
        {
            var product = CreateProductReference(productCode);
            
            var statistics = this.GetProductStatistics(product);
            var reviews = this.GetProductReviews(product);

            if (reviews == null)
            {
                return null;
            }

            return new ReviewsViewModel(productCode, reviews.FirstOrDefault().Extension.ProductName)
            {
                Statistics = ViewModelAdapter.Adapt(statistics),
                Reviews = ViewModelAdapter.Adapt(reviews).ToList()
            };            
        }

        public void Comment(ReviewCommentsViewModel comment)
        {
            // set the 'created' time
            comment.Created = DateTime.Now.ToString("MMMM dd, yyyy");

            // load the review composite
            var reviewId = CommentId.Create(comment.ReviewId);
            var review = _commentService.Get<Review>(reviewId);
            if (review.Extension.Comments == null)
            {
                review.Extension.Comments = new List<ReviewComment>();
            }
            // Add to beginning of the list
            review.Extension.Comments.Insert(0, new ReviewComment
            {
                ProductId = comment.ProductId,
                ReviewId = comment.ReviewId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.AuthorName,
                Text = comment.Text,
                Created = Convert.ToDateTime(comment.Created)
            });
            // update/save the review
            _commentService.Update(review.Data, review.Extension);
        }

        /// <summary>
        /// Gets the rating statistics for the identified product
        /// </summary>
        /// <param name="product">Reference identifying the product</param>
        /// <returns>Rating statistics for the product</returns>
        private RatingStatistics GetProductStatistics(Reference product)
        {
            var statisticsCriteria = new Criteria<RatingStatisticsFilter>()
            {
                Filter = new RatingStatisticsFilter()
                {
                    Targets = new[] { product }
                },
                PageInfo = new PageInfo()
                {
                     PageSize = 1
                }
            };

            return this._ratingService.Get(statisticsCriteria).Results.FirstOrDefault();
        }

        /// <summary>
        /// Gets a collection of reviews for the identified product.
        /// </summary>
        /// <param name="product">Reference identifying the product</param>
        /// <returns>Collection of reviews for the product</returns>
        private IEnumerable<Composite<Comment, Review>> GetProductReviews(EPiServer.Social.Common.Reference product)
        {
            var commentCriteria = new CompositeCriteria<CommentFilter, Review>()
            {
                Filter = new CommentFilter
                {
                    Parent = product
                },
                PageInfo = new PageInfo
                {
                     PageSize = 20
                },
                OrderBy = new List<SortInfo>
                {
                    new SortInfo(CommentSortFields.Created, false)
                }
            };

            return this._commentService.Get(commentCriteria).Results;
        }

        /// <summary>
        /// Creates a reference identifying a review contributor.
        /// </summary>
        /// <param name="nickname">Nickname identifying the review contributor</param>
        /// <returns>Reference identifying a review contributor</returns>
        private static EPiServer.Social.Common.Reference CreateContributorReference(string nickname)
        {
            return EPiServer.Social.Common.Reference.Create($"visitor://{nickname}");
        }

        /// <summary>
        /// Creates a reference identifying a product.
        /// </summary>
        /// <param name="productCode">Content code identifying the product</param>
        /// <returns>Reference identifying a product</returns>
        private static EPiServer.Social.Common.Reference CreateProductReference(string productCode)
        {
            return EPiServer.Social.Common.Reference.Create($"product://{productCode}");
        }

        private static List<ReviewSecondaryRating> SetSecondaryRatings(ReviewSubmissionViewModel review)
        {
            if (review == null)
                return new List<ReviewSecondaryRating>();

            var type = review.GetType();
            var list = new List<ReviewSecondaryRating>();
            foreach (var info in type.GetProperties(BindingFlags.Public| BindingFlags.Instance))
            {
                var name = info.Name;
                var val = info.GetValue(review, null);
                if (info.PropertyType.Name.Equals("ReviewSecondaryRatingViewModel"))
                {
                    list.Add(new ReviewSecondaryRating {Label = name, RatingValue = val.ToString()});
                }
            }


            
            
            //foreach (var question in review.SecondaryRatings)
            //{
            //    list.Add(new ReviewSecondaryRating {Label = question.Label, RatingValue = question.RatingValue});
            //}
            return list;
        }
    }
}