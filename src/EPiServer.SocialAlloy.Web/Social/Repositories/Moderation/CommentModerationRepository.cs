﻿using EPiServer.Social.Common;
using EPiServer.Social.Moderation.Core;
using EPiServer.SocialAlloy.ExtensionData.Membership;
using EPiServer.SocialAlloy.Web.Social.Adapters.Groups;
using EPiServer.SocialAlloy.Web.Social.Adapters.Moderation;
using EPiServer.SocialAlloy.Web.Social.Common.Exceptions;
using EPiServer.SocialAlloy.Web.Social.Models;
using EPiServer.SocialAlloy.Web.Social.Models.Comments;
using EPiServer.SocialAlloy.Web.Social.Models.Groups;
using EPiServer.SocialAlloy.Web.Social.Models.Moderation;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.SocialAlloy.Web.Social.Repositories.Moderation
{
    /// <summary>
    /// The CommunityMembershipModerationRepository implements operations that manage community membership moderation with the Episerver Social Framework.
    /// </summary>
    public class CommentModerationRepository : ICommentModerationRepository
    {
        private readonly IWorkflowService workflowService;
        private readonly IWorkflowItemService workflowItemService;
        private readonly ICommunityMemberRepository memberRepository;
        private readonly IUserRepository userRepository;
        private CommunityMembershipWorkflowAdapter workflowAdapter;
        private CommunityMemberAdapter memberAdapter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workflowService">Moderation workflow service supporting this application</param>
        /// <param name="workflowItemService">Moderation workflow item service supporting this application</param>
        /// <param name="memberService">Member service supporting this application</param>
        public CommentModerationRepository(IWorkflowService workflowService, IWorkflowItemService workflowItemService, ICommunityMemberRepository memberRepository, IUserRepository userRepository)
        {
            this.workflowService = workflowService;
            this.workflowItemService = workflowItemService;
            this.memberRepository = memberRepository;
            this.userRepository = userRepository;
            this.workflowAdapter = new CommunityMembershipWorkflowAdapter();
            this.memberAdapter = new CommunityMemberAdapter();
        }

        /// <summary>
        /// Adds a workflow to the underlying repository
        /// </summary>
        public void AddWorkflow()
        {
            // Define the transitions for workflow:            
            // Pending -> (Accept) -> Accepted
            //     |                      |-- (Approve) -> Approved
            //     |                       `- (Reject)  -> Rejected
            //      `---> (Ignore) -> Rejected

            var workflowTransitions = new List<WorkflowTransition>
            {
                new WorkflowTransition(new WorkflowState("Pending"),  new WorkflowState("Approved"), new WorkflowAction("Approve")),
                new WorkflowTransition(new WorkflowState("Pending"),  new WorkflowState("Rejected"), new WorkflowAction("Reject"))
            };

            // Save the new workflow with custom extension data which 
            // identifies the community it is intended to be associated with.

            var commentsWorkflow = new Workflow(
                "Comments",
                workflowTransitions,
                new WorkflowState("Pending")
            );

            //var workflowExtension = new CommentsExtension { IsImage = false };


            if (commentsWorkflow != null)
            {
                try
                {
                    this.workflowService.Add(commentsWorkflow);
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
        }

        /// <summary>
        /// Retrieves specific workflowitem extension data from the underlying repository.
        /// </summary>
        /// <param name="userId">The unique id of the user under moderation.</param>
        /// <param name="communityId">The unique id of the community to which membership has been requested.</param>
        /// <returns>The state of the request</returns>
        public string GetCommentRequestState(string userId, string communityId)
        {
            var compositeMember = GetComposite(userId, communityId);
            return compositeMember == null ? null : compositeMember.Data.State.Name;
        }

        /// <summary>
        /// Retrieves specific workflowitem extension data from the underlying repository
        /// </summary>
        /// <param name="userId">The unique id of the user under moderation.</param>
        /// <param name="communityId">The unique id of the community to which membership has been requested.</param>
        /// <returns>AddMemberRequest: the workflowItem extension data</returns>
        private AddMemberRequest GetCommentRequest(string userId, string communityId)
        {
            var compositeMember = GetComposite(userId, communityId);
            return compositeMember == null ? null : compositeMember.Extension;
        }

        /// <summary>
        /// Retrieves specific workflowitem and extension data from the underlying repository
        /// </summary>
        /// <param name="user">The user under moderation</param>
        /// <param name="group">The group that membership is being moderated</param>
        /// <returns>composite of WorkflowItem and AddMemberRequest</returns>
        private Composite<WorkflowItem, AddMemberRequest> GetComposite(string user, string group)
        {
            Composite<WorkflowItem, AddMemberRequest> memberRequest = null;

            //Construct a filter to return the desired target under moderation
            var filter = new CompositeCriteria<WorkflowItemFilter, AddMemberRequest>();
            filter.Filter.Target = Reference.Create(CreateUri(group, user));

            try
            {
                //retrieve the first workflow that matches the target filter 
                memberRequest =  this.workflowItemService.Get(filter).Results.LastOrDefault();
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

            return memberRequest;
        }

        /// <summary>
        /// Returns a view model supporting the presentation of group
        /// membership moderation information.
        /// </summary>
        /// <param name="workflowId">Identifier for the selected membership moderation workflow</param>
        /// <returns>View model of moderation information</returns>
        public CommentModerationViewModel Get(string workflowId)
        {
            try
            {
                // Retrieve a collection of all workflows in the system with MembershipModeration extension data.
                var selectedWorkflow =  workflowService.Get(new WorkflowId(workflowId));
             
                // Retrieve the current state for all comment requests
                // under the selected moderation workflow.

                var currentWorkflowItems = this.GetWorkflowItemsFor(selectedWorkflow);

                var workflowItemAdapter = new CommentRequestAdapter(selectedWorkflow, this.userRepository);

                return new CommentModerationViewModel
                {
                    Workflows = allWorkflows.Select(workflowAdapter.Adapt),
                    SelectedWorkflow = workflowAdapter.Adapt(selectedWorkflow),
                    Items = currentWorkflowItems.Select(workflowItemAdapter.Adapt)
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
        /// Submits a membership request to the specified group's
        /// moderation workflow for approval.
        /// </summary>
        /// <param name="member">The member information for the membership request</param>
        public void AddAModeratedComment(PageComment comment)
        {
            // Define a unique reference representing the entity
            // under moderation. Note that this entity may be
            // transient or may not yet have been assigned a
            // unique identifier. Defining an item reference allows
            // you to bridge this gap.

            // For example: "members:/{group-id}/{user-reference}"

            var targetReference = this.CreateUri(comment.Target, comment.AuthorId);

            // Retrieve the workflow supporting moderation of
            // membership for the group to which the user is
            // being added.

            var moderationWorkflow = this.GetWorkflowFor(member.GroupId);

            // The workflow defines the intial (or 'start') state
            // for moderation.

            var initialState = moderationWorkflow.InitialState;

            // Create a new workflow item...

            var workflowItem = new WorkflowItem(
                WorkflowId.Create(moderationWorkflow.Id),      // ...under the group's moderation workflow
                new WorkflowState(initialState),               // ...in the workflow's initial state
                Reference.Create(targetReference)              // ...identified with this reference
            );

            var memberRequest = memberAdapter.Adapt(member);

                try
                {
                    this.workflowItemService.Add(workflowItem, memberRequest);
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
        /// Takes action on the specified workflow item, representing a
        /// membership request.
        /// </summary>
        /// <param name="workflowId">The id of the workflow </param>
        /// <param name="action">The moderation action to be taken</param>
        /// <param name="userId">The unique id of the user under moderation.</param>
        /// <param name="communityId">The unique id of the community to which membership has been requested.</param>
        public void Moderate(string workflowId, string action, string userId, string communityId)
        {
            var membershipRequest = GetCommentRequest(userId, communityId);
            var populatedWorkflowId = WorkflowId.Create(workflowId);

            var requestReference = Reference.Create(CreateUri(membershipRequest.Group, membershipRequest.User));

            try
            {
                var transitionToken = this.workflowService.BeginTransitionSession(populatedWorkflowId, requestReference);
                try
                {
                    // Retrieve the moderation workflow associated with
                    // the item to be acted upon.

                    var workflow = this.workflowService.Get(populatedWorkflowId);

                    // Leverage the workflow to determine what the
                    // resulting state of the item will be upon taking 
                    // the specified action.

                    //retrieve the current state of the workflow item once the begintransitionsession begins.
                    var filter = new WorkflowItemFilter { Target = requestReference };
                    var criteria = new Criteria<WorkflowItemFilter> { Filter = filter };
                    var workflowItem = this.workflowItemService.Get(criteria).Results.Last();

                    // Example: Current State: "Pending", Action: "Approve" => Transitioned State: "Approved"
                    var transitionedState = workflow.Transition(workflowItem.State, new WorkflowAction(action));

                    var subsequentWorkflowItem = new WorkflowItem(
                        workflow.Id,
                        transitionedState,
                        requestReference
                    );

                    this.workflowItemService.Add(subsequentWorkflowItem, membershipRequest, transitionToken);

                    // Perform any application logic given the item's
                    // new state.

                    if (this.IsApproved(subsequentWorkflowItem.State))
                    {
                        memberRepository.Add(memberAdapter.Adapt(membershipRequest));
                    }
                }
                finally
                {
                    this.workflowService.EndTransitionSession(transitionToken);
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
        /// Returns the moderation workflow supporting the specified group.
        /// </summary>
        /// <param name="group">ID of the group</param>
        /// <returns>Moderation workflow supporting the specified group</returns>
        private CommentMembershipWorkflow GetWorkflowFor(string group)
        {
            CommunityMembershipWorkflow expectedSocialWorkflow = null;
            IEnumerable<Workflow> listOfWorkflow = Enumerable.Empty<Workflow>();

            var filterWorkflowsByGroup =
                FilterExpressionBuilder<MembershipModeration>.Field(m => m.)
                                                             .EqualTo(group);

            var criteria = new CompositeCriteria<WorkflowFilter, MembershipModeration>
            {
                PageInfo = new PageInfo { PageSize = 1 },
                ExtensionFilter = filterWorkflowsByGroup
            };

            try
            {
                listOfWorkflow = this.workflowService.Get(criteria).Results;
                if (listOfWorkflow.Count() > 0)
                {
                    var workflow = listOfWorkflow.First().Data;
                    expectedSocialWorkflow = new CommunityMembershipWorkflow(workflow.Id.Id, workflow.Name, workflow.InitialState.Name);
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

            return expectedSocialWorkflow;
        }

        /// <summary>
        /// Returns true if the specified group has a moderation workflow,
        /// false otherwise.
        /// </summary>
        /// <param name="groudId">ID of the group</param>
        /// <returns>True if the specified group has a moderation workflow, false otherwise</returns>
        public bool IsModerated(string groudId)
        {
            return this.GetWorkflowFor(groudId) != null;
        }

        /// <summary>
        /// Creates a unique uri to be used with to track the progression of a member being moderated for group admission 
        /// </summary>
        /// <param name="group">Id of the group that a member is trying to join</param>
        /// <param name="user">The name of the member that is trying to join a group</param>
        /// <returns></returns>
        private string CreateUri(string group, string user)
        {
            return
               string.Format(
                   "members://{0}/{1}",
                   group,
                   user
               );
        }

        /// <summary>
        /// Retrieves a collection of the first 30 workflows in
        /// the system.
        /// </summary>
        /// <returns>Collection of workflows</returns>
        private IEnumerable<Workflow> GetWorkflows()
        {
            var criteria = new CompositeCriteria<WorkflowFilter, MembershipModeration>
            {
                PageInfo = new PageInfo { PageSize = 30 }
            };

            return this.workflowService.Get(criteria).Results.Select(x => x.Data);
        }

        /// <summary>
        /// Retrieves the first 30 current workflow items, associated with the
        /// specified workflow, which represent group membership requests.
        /// </summary>
        /// <param name="workflow">Workflow from which to retrieve items</param>
        /// <returns>Collection of workflow items</returns>
        private IEnumerable<Composite<WorkflowItem, AddMemberRequest>> GetWorkflowItemsFor(Workflow workflow)
        {
            IEnumerable<Composite<WorkflowItem, AddMemberRequest>> items;

            if (workflow == null)
            {
                items = new List<Composite<WorkflowItem, AddMemberRequest>>();
            }
            else
            {
                var criteria = new CompositeCriteria<WorkflowItemFilter, AddMemberRequest>
                {
                    Filter = new WorkflowItemFilter
                    {
                        ExcludeHistoricalItems = true,      // Include only the current state for the requests
                        Workflow = workflow.Id,             // Include only items for the selected group's workflow
                    },
                    PageInfo = new PageInfo { PageSize = 30 },   // Limit to 30 items                    
                };

                // Order the results alphabetically by their state and then
                // by the date on which they were created.

                criteria.OrderBy.Add(new SortInfo(WorkflowItemSortFields.State, true));
                criteria.OrderBy.Add(new SortInfo(WorkflowItemSortFields.Created, true));

                items = this.workflowItemService.Get(criteria).Results;
            }

            return items;
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
    }
}
