using EPiServer.ServiceLocation;
using EPiServer.Social.Comments.Core;
using EPiServer.Social.Common;
using EPiServer.Social.Moderation.Core;
using EPiServer.SocialAlloy.ExtensionData.Comment;
using EPiServer.SocialAlloy.Web.Social.Repositories;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using System;
using EPiServer.SocialAlloy.Web.Social.Models;
using EPiServer.SocialAlloy.Web.Social.Common.Exceptions;
using EPiServer.Social.ActivityStreams.Core;

namespace EPiServer.SocialAlloy.Web.Social.Controllers
{
    /// <summary>
    /// The ModerationController handles the display of information
    /// for membership requests under moderation and faciliates
    /// the actions which may be taken upon them.
    /// </summary>
    public class DeleteAllCommentsController : Controller
    {

        /// <summary>
        /// Presents a view which includes a list of the requests under
        /// moderation for the specified workflow. If no workflow is
        /// specified, the first one in the system will be selected.
        /// </summary>
        /// <param name="selectedWorkflow">ID of the selected moderation workflow</param>        
        public ActionResult Index(string selectedWorkflow)
        {
            // Remove All The Visible Comments
            var page = ServiceLocator.Current.GetInstance<ICommentService>();

            var truth = Visibility.Visible;

            var commentfilter = new Criteria<CommentFilter>
            {
                Filter = new CommentFilter
                {
                    Visibility = truth

                }
            };

            var commentResults = page.Get(commentfilter);

            foreach (Comment comment in commentResults.Results)
            {
                page.Remove(comment.Id);
            }

            // Remove all the Invisible Comments
            truth = Visibility.NotVisible;

            var commentfilter2 = new Criteria<CommentFilter>
            {
                Filter = new CommentFilter
                {
                    Visibility = truth

                }
            };

            var commentResults2 = page.Get(commentfilter2);

            foreach (Comment comment in commentResults2.Results)
            {
                page.Remove(comment.Id);
            }

            // Remove all Comment Request 

            IEnumerable<Composite<WorkflowItem, AddCommentRequest>> items;
            var workflowService = ServiceLocator.Current.GetInstance<IWorkflowService>();

            var wfcriteria = new Criteria<WorkflowFilter>
            {
                Filter = new WorkflowFilter
                {
                    Name = "Comments Workflow"
                }
            };

            ResultPage<Workflow> pageOfWorkflows = workflowService.Get(wfcriteria);

            Workflow commentWorkflow = pageOfWorkflows.Results.First<Workflow>();

            var workflowItemService = ServiceLocator.Current.GetInstance<IWorkflowItemService>();

            if (commentWorkflow == null)
            {
                items = new List<Composite<WorkflowItem, AddCommentRequest>>();
            }
            else
            {
                var criteria = new CompositeCriteria<WorkflowItemFilter, AddCommentRequest>
                {
                    Filter = new WorkflowItemFilter
                    {
                        ExcludeHistoricalItems = false      // Include only the current state for the requests
                    },
                    PageInfo = new PageInfo { PageSize = 30 },   // Limit to 30 items                    
                };

                // Order the results alphabetically by their state and then
                // by the date on which they were created.

                criteria.OrderBy.Add(new SortInfo(WorkflowItemSortFields.State, true));
                criteria.OrderBy.Add(new SortInfo(WorkflowItemSortFields.Created, true));

                items = workflowItemService.Get(criteria).Results;
            }

            // Now Delete All the items

            foreach (var item in items)
            {
                workflowItemService.Remove(item.Data.Id);
            }

            // Remove Current User Feed Data.

            var userRepository = ServiceLocator.Current.GetInstance<IUserRepository>();
            var feedService = ServiceLocator.Current.GetInstance<IFeedService>();
            var feedItems = new List<Composite<FeedItem, CommunityActivity>>();

            var userId = userRepository.GetUserId(this.User);

            try
            {
                feedItems = feedService.Get(
                    new CompositeCriteria<FeedItemFilter, CommunityActivity>
                    {
                        PageInfo = new PageInfo
                        {
                            PageSize = 300
                        },
                        IncludeSubclasses = true,
                        Filter = new FeedItemFilter
                        {
                            Subscriber = Reference.Create(userId)
                        }
                        ,
                        OrderBy = { new SortInfo(FeedItemSortFields.ActivityDate, false) }
                    }
                ).Results.ToList();
            }
            catch (SocialAuthenticationException ex)
            {
                throw new SocialRepositoryException("The application failed to authenticate with Episerver Social.", ex);
            }
            catch (MaximumDataSizeExceededException ex)
            {
                throw new SocialRepositoryException("The application request was deemed too large for Episerver Social.", ex);
            }
            catch (SocialCommunicationException ex)
            {
                throw new SocialRepositoryException("The application failed to communicate with Episerver Social.", ex);
            }
            catch (SocialException ex)
            {
                throw new SocialRepositoryException("Episerver Social failed to process the application request.", ex);
            }

            if (feedItems != null)
            {
                foreach (var item in feedItems)
                {
                 
                }
            }



            return Content("Done");
        }

    }

}