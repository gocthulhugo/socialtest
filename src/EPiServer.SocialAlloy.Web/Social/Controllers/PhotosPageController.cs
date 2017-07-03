using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.ServiceLocation;
using EPiServer.Social.Comments;
using EPiServer.Social.Comments.Core;
using EPiServer.Social.Common;
using EPiServer.SocialAlloy.Web.Models.ViewModels;
using EPiServer.SocialAlloy.Web.Social.Models;
using EPiServer.SocialAlloy.Web.Social.Models.Comments;
using EPiServer.SocialAlloy.Web.Social.Models.Photos;
using EPiServer.SocialAlloy.Web.Social.Pages;
using EPiServer.SocialAlloy.Web.Social.Repositories;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;

namespace EPiServer.SocialAlloy.Web.Social.Controllers
{
    /// <summary>
    /// The PhotosPageController handles rendering of social photos for a given product
    /// </summary>
    public class PhotosPageController : PageController<PhotosPage>
    {
        private readonly IPageCommentRepository commentRepository;
        private readonly IPageRepository pageRepository;
        private readonly IPageRouteHelper pageRouteHelper;
        private readonly ICommentService commentService;

        public PhotosPageController()
        {
            this.commentRepository = ServiceLocator.Current.GetInstance<IPageCommentRepository>();
            this.pageRepository = ServiceLocator.Current.GetInstance<IPageRepository>();
            this.pageRouteHelper = ServiceLocator.Current.GetInstance<IPageRouteHelper>();
            this.commentService = ServiceLocator.Current.GetInstance<ICommentService>();
        }

        /// <summary>
        /// Render the social photos page view
        /// </summary>
        /// <param name="currentPage">The current social photos page.</param>
        /// <returns></returns>
        public ActionResult Index(PhotosPage currentPage)
        {
            var pageViewModel = new PageViewModel<PhotosPage>(currentPage);
            // can create a reference like this so you don't need to use the epi pages
            //var parent = Reference.Create($"product://{product}/{facet}");
            var pageReference = this.pageRouteHelper.PageLink;
            var pageId = this.pageRepository.GetPageId(pageReference);
            
            var comments = this.GetComments(); // TODO: Fix this to include product id. For now I'm happy to just get the comments.

            // Fake the photos
            var photos = new Photos
            {
                ProductId = "56146",
                ProductName = "BBK 73mm Throttle Body (11-17 V6)",
                CustomerPhotos = new List<Photo>(),
                PageId = pageId
            };
            photos.CustomerPhotos.Add(new Photo
            {
                PhotoId = 48482,
                Uri = "https://acdn.americanmuscle.com/i/thumb/20120626122409_4776.jpg",
                CustomerName = "PonyBoyZPunisher",
                Created = Convert.ToDateTime("6/26/2012"),
                ProductName = "BBK 73mm Throttle Body (11-17 V6)",
                ProductId = "56146",
                CustomerId = 577404,
                Comments = comments.Where(x => x.PhotoId == 48482).ToList()
            });
            photos.CustomerPhotos.Add(new Photo
            {
                PhotoId = 80835,
                Uri = "https://acdn.americanmuscle.com/i/thumb/20131019214442_4192.jpg",
                CustomerName = "DaveB111",
                Created = Convert.ToDateTime("10/19/2013"),
                ProductName = "BBK 73mm Throttle Body (11-17 V6)",
                ProductId = "56146",
                CustomerId = 930383,
                Comments = comments.Where(x => x.PhotoId == 80835).ToList()
            });
            photos.CustomerPhotos.Add(new Photo
            {
                PhotoId = 85889,
                Uri = "https://acdn.americanmuscle.com/i/thumb/20131228021023_4145.jpg",
                CustomerName = "DuggieC",
                Created = Convert.ToDateTime("12/28/2013"),
                ProductName = "BBK 73mm Throttle Body (11-17 V6)",
                ProductId = "56146",
                CustomerId = 1068458,
                Comments = comments.Where(x => x.PhotoId == 85889).ToList()
            });
            photos.CustomerPhotos.Add(new Photo
            {
                PhotoId = 133722,
                Uri = "https://acdn.americanmuscle.com/i/thumb/9318568dc531af4b2_20160106205353.jpg",
                CustomerName = "Jack",
                Created = Convert.ToDateTime("1/6/2016"),
                ProductName = "BBK 73mm Throttle Body (11-17 V6)",
                ProductId = "56146",
                CustomerId = 2272778,
                Comments = comments.Where(x => x.PhotoId == 133722).ToList()
            });
            photos.CustomerPhotos.Add(new Photo
            {
                PhotoId = 51653,
                Uri = "https://acdn.americanmuscle.com/i/thumb/20120807081503_8200.jpg",
                CustomerName = "nathan beagan",
                Created = Convert.ToDateTime("8/7/2012"),
                ProductName = "BBK 73mm Throttle Body (11-17 V6)",
                ProductId = "56146",
                CustomerId = -1,
                Comments = comments.Where(x => x.PhotoId == 51653).ToList()
            });
            photos.CustomerPhotos.Add(new Photo
            {
                PhotoId = 79336,
                Uri = "https://acdn.americanmuscle.com/i/thumb/20130927005453_6560.jpg",
                CustomerName = "Cobra10",
                Created = Convert.ToDateTime("9/27/2013"),
                ProductName = "BBK 73mm Throttle Body (11-17 V6)",
                ProductId = "56146",
                CustomerId = 291065,
                Comments = comments.Where(x => x.PhotoId == 79336).ToList()
            });
            pageViewModel.CurrentPage.ProductPhotos = photos;
            return View("~/Views/PhotosPage/Index.cshtml", pageViewModel);
        }


        private List<PageComment> GetComments()
        {
            var list = new List<PageComment>();
            try
            {
                // visible + invisible
                var commentfilter = new Criteria<CommentFilter>
                {
                    Filter = new CommentFilter
                    {
                        Visibility = Visibility.Visible
                    }
                };

                var commentResults = commentService.Get(commentfilter);

                foreach (var comment in commentResults.Results)
                {
                    var composite = commentService.Get<CommentsExtension>(comment.Id);
                    var pageComment = commentRepository.AdaptComment(composite);
                    if (pageComment != null)
                        list.Add(pageComment);
                }
                return list;
            }
            catch(Exception ex)
            {
                //Do something
            }
            return list;
        }
    }
}