using EPiServer.ServiceLocation;
using EPiServer.Social.Common;
using EPiServer.Social.Moderation.Core;
using EPiServer.SocialAlloy.ExtensionData.Comment;
using EPiServer.SocialAlloy.Web.Social.Models.Comments;
using EPiServer.SocialAlloy.Web.Social.Models.Moderation;
using EPiServer.SocialAlloy.Web.Social.Repositories;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using EPiServer.SocialAlloy.Web.Social.Common.Exceptions;
using EPiServer.Social.Comments.Core;
using EPiServer.SocialAlloy.Web.Social.Models;

namespace EPiServer.SocialAlloy.Web.Social.Controllers
{
    /// <summary>
    /// The ModerationController handles the display of information
    /// for membership requests under moderation and faciliates
    /// the actions which may be taken upon them.
    /// </summary>
    public class CommentModerationController : Controller
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public CommentModerationController()
        {
        }

        /// <summary>
        /// Presents a view which includes a list of the requests under
        /// moderation for the specified workflow. If no workflow is
        /// specified, the first one in the system will be selected.
        /// </summary>
        /// <param name="selectedWorkflow">ID of the selected moderation workflow</param>        
        public ActionResult Index(string selectedWorkflow)
        {
            var model = GetViewModel();
            return View("~/Views/Social/CommentModeration/Index.cshtml", model);
        }

        /// <summary>
        /// Retrieves relevant membership information and takes the specified action on a membership request under moderation.
        /// </summary>
        /// <param name="userId">User associated with the membership request</param>
        /// <param name="communityId">Community associated with the membership request</param>
        /// <param name="workflow">Workflow associated with the membership request</param>
        /// <param name="workflowAction">Action to be taken on the membership request</param>
        /// <returns>ActionResult</returns>
        [HttpPost]
        public ActionResult Index(string userId, string commentId, string workflowAction)
        {
            Moderate(workflowAction, userId, commentId);

            return RedirectToAction("Index", new RouteValueDictionary());
        }

        public CommentModerationViewModel GetViewModel()
        {
            try
            {
                var workflowService = ServiceLocator.Current.GetInstance<IWorkflowService>();

                var criteria = new Criteria<WorkflowFilter>
                {
                    Filter = new WorkflowFilter
                    {
                        Name = "Comments Workflow"
                    }
                };

                ResultPage<Workflow> pageOfWorkflows = workflowService.Get(criteria);

                Workflow commentWorkflow = pageOfWorkflows.Results.First<Workflow>();

                // Retrieve the current state for all comment requests
                // under the selected moderation workflow.

                var currentWorkflowItems = this.GetWorkflowItemsFor(commentWorkflow);


                return new CommentModerationViewModel
                {
                    Items = currentWorkflowItems.Select(Adapt)
                };
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
        }

        /// <summary>
        /// Retrieves the first 30 current workflow items, associated with the
        /// specified workflow, which represent group membership requests.
        /// </summary>
        /// <param name="workflow">Workflow from which to retrieve items</param>
        /// <returns>Collection of workflow items</returns>
        private IEnumerable<Composite<WorkflowItem, AddCommentRequest>> GetWorkflowItemsFor(Workflow workflow)
        {
            IEnumerable<Composite<WorkflowItem, AddCommentRequest>> items;

            var workflowItemService = ServiceLocator.Current.GetInstance<IWorkflowItemService>();

            if (workflow == null)
            {
                items = new List<Composite<WorkflowItem, AddCommentRequest>>();
            }
            else
            {
                var criteria = new CompositeCriteria<WorkflowItemFilter, AddCommentRequest>
                {
                    Filter = new WorkflowItemFilter
                    {
                        ExcludeHistoricalItems = true      // Include only the current state for the requests
                    },
                    PageInfo = new PageInfo { PageSize = 30 },   // Limit to 30 items                    
                };

                // Order the results alphabetically by their state and then
                // by the date on which they were created.

                criteria.OrderBy.Add(new SortInfo(WorkflowItemSortFields.State, true));
                criteria.OrderBy.Add(new SortInfo(WorkflowItemSortFields.Created, true));

                items = workflowItemService.Get(criteria).Results;
            }

            return items;
        }

        /// <summary>
        /// Converts a composite WorkflowItem with extension data of type AddMemberRequestModel into a CommunityMembershipRequest
        /// </summary>
        /// <param name="item">Composite item to be adapted into a CommunityMembershipRequest</param>
        /// <returns>CommunityMembershipRequest</returns>
        public CommentRequest Adapt(Composite<WorkflowItem, AddCommentRequest> item)
        {
            var workflowService = ServiceLocator.Current.GetInstance<IWorkflowService>();

            var criteria = new Criteria<WorkflowFilter>
            {
                Filter = new WorkflowFilter
                {
                    Name = "Comments Workflow"
                }
            };

            ResultPage<Workflow> pageOfWorkflows = workflowService.Get(criteria);

            Workflow commentWorkflow = pageOfWorkflows.Results.First<Workflow>();

            var user = item.Extension.User;
            var userRepository = ServiceLocator.Current.GetInstance<IUserRepository>();
            var userName = userRepository.ParseUserUri(user);

            return new CommentRequest
            {
                User = user,
                WorkflowId = item.Data.Workflow.ToString(),
                Created = item.Data.Created.ToLocalTime(),
                State = item.Data.State.Name,
                Actions = commentWorkflow.ActionsFor(item.Data.State).Select(a => a.Name),
                UserName = userName,
                Body = item.Extension.Body,
                CommentId = item.Extension.CommentId,
                IsImage = item.Extension.IsImage
            };
        }

        /// <summary>
        /// Takes action on the specified workflow item, representing a
        /// membership request.
        /// </summary>
        /// <param name="workflowId">The id of the workflow </param>
        /// <param name="action">The moderation action to be taken</param>
        /// <param name="userId">The unique id of the user under moderation.</param>
        /// <param name="communityId">The unique id of the community to which membership has been requested.</param>
        public void Moderate( string action, string userId, string commentId)
        {

            var memberRepository = ServiceLocator.Current.GetInstance<ICommunityMemberRepository>();

            var workflowService = ServiceLocator.Current.GetInstance<IWorkflowService>();
            var criteria = new Criteria<WorkflowFilter>
            {
                Filter = new WorkflowFilter
                {
                    Name = "Comments Workflow"
                }
            };

            ResultPage<Workflow> pageOfWorkflows = workflowService.Get(criteria);

            Workflow commentWorkflow = pageOfWorkflows.Results.First<Workflow>();

            var requestReference = Reference.Create(commentId);

            var commentRequest = GetCommentRequest(userId, commentId);

            try
            {
                var transitionToken = workflowService.BeginTransitionSession(commentWorkflow.Id, requestReference);
                try
                {
                    // Retrieve the moderation workflow associated with
                    // the item to be acted upon.

                    var workflow = workflowService.Get(commentWorkflow.Id);

                    // Leverage the workflow to determine what the
                    // resulting state of the item will be upon taking 
                    // the specified action.

                    IWorkflowItemService workflowItemService = ServiceLocator.Current.GetInstance<IWorkflowItemService>();

                    //retrieve the current state of the workflow item once the begintransitionsession begins.
                    var filter = new WorkflowItemFilter { Target = requestReference };
                    var wficriteria = new Criteria<WorkflowItemFilter> { Filter = filter };
                    var workflowItem = workflowItemService.Get(wficriteria).Results.Last();

                    // Example: Current State: "Pending", Action: "Approve" => Transitioned State: "Approved"
                    var transitionedState = workflow.Transition(workflowItem.State, new WorkflowAction(action));

                    var subsequentWorkflowItem = new WorkflowItem(
                        workflow.Id,
                        transitionedState,
                        requestReference
                    );

                    workflowItemService.Add(subsequentWorkflowItem, commentRequest, transitionToken);

                    // Perform any application logic given the item's
                    // new state.

                    if (IsApproved(subsequentWorkflowItem.State))
                    {
                        // TODO! Alter the comment to be visible!
                        UpdateComment(commentId);
                    }
                }
                finally
                {
                    workflowService.EndTransitionSession(transitionToken);
                }
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
        }

        /// <summary>
        /// Retrieves specific workflowitem extension data from the underlying repository
        /// </summary>
        /// <param name="userId">The unique id of the user under moderation.</param>
        /// <param name="communityId">The unique id of the community to which membership has been requested.</param>
        /// <returns>AddMemberRequest: the workflowItem extension data</returns>
        private AddCommentRequest GetCommentRequest(string userId, string commentId)
        {
            var compositeMember = GetComposite(userId, commentId);
            return compositeMember == null ? null : compositeMember.Extension;
        }

        /// <summary>
        /// Retrieves specific workflowitem and extension data from the underlying repository
        /// </summary>
        /// <param name="user">The user under moderation</param>
        /// <param name="group">The group that membership is being moderated</param>
        /// <returns>composite of WorkflowItem and AddMemberRequest</returns>
        private Composite<WorkflowItem, AddCommentRequest> GetComposite(string user, string commentid)
        {
            IWorkflowItemService workflowItemService = ServiceLocator.Current.GetInstance<IWorkflowItemService>();

            Composite<WorkflowItem, AddCommentRequest> commentRequest = null;

            //Construct a filter to return the desired target under moderation
            var filter = new CompositeCriteria<WorkflowItemFilter, AddCommentRequest>();
            filter.Filter.Target = Reference.Create(commentid);

            try
            {
                //retrieve the first workflow that matches the target filter 
                commentRequest = workflowItemService.Get(filter).Results.LastOrDefault();
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

            return commentRequest;
        }

        /// <summary>
        /// Returns true if the specified WorkflowState instance represents
        /// the "approved" state, false otherwise.
        /// </summary>
        /// <param name="state">State to verify</param>
        /// <returns>True if the specified WorkflowState instance represents the "Approved" state, false otherwise</returns>
        private bool IsApproved(WorkflowState state)
        {
            return state.Name.ToLower() == "approved";
        }

        private void UpdateComment(string commentId)
        {
            ICommentService commentService = ServiceLocator.Current.GetInstance<ICommentService>();
            var currentCommentId = CommentId.Create(commentId);

            var currentComment = commentService.Get<CommentsExtension>(currentCommentId);

            Comment updatedComment = new Comment(currentComment.Data.Id, currentComment.Data.Parent, currentComment.Data.Author, currentComment.Data.Body, true);
            CommentsExtension updatedCommentExt = new CommentsExtension
            {
                IsImage = currentComment.Extension.IsImage
            };

            commentService.Update(updatedComment, updatedCommentExt);

            var activityRepository = ServiceLocator.Current.GetInstance<ICommunityActivityRepository>();

            try
            {
                var commentActivity = new PageCommentActivity { Body = updatedComment.Body, IsImage = updatedCommentExt.IsImage };

                activityRepository.Add(updatedComment.Author.Id, updatedComment.Parent.Id, commentActivity);
            }
            catch (SocialRepositoryException ex)
            {
            }
        }
    }
}
