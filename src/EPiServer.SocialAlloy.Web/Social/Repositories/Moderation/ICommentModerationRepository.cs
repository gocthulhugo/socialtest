﻿using EPiServer.SocialAlloy.Web.Social.Models;
using EPiServer.SocialAlloy.Web.Social.Models.Groups;
using EPiServer.SocialAlloy.Web.Social.Models.Moderation;

namespace EPiServer.SocialAlloy.Web.Social.Repositories
{
    /// <summary>
    /// The interface describing operations that manage community membership moderation. 
    /// </summary>
    public interface ICommentModerationRepository
    {
        /// <summary>
        /// Adds a new workflow to the underlying repository for comments. 
        /// </summary>
        void AddWorkflow();

        /// <summary>
        /// Submits a comment request to
        /// moderation workflow for approval.
        /// </summary>
        /// <param name="comment">The comment information for the request</param>
        void AddAModeratedComment(PageComment comment);

        /// <summary>
        /// Returns a view model supporting the presentation of community
        /// membership moderation information.
        /// </summary>
        /// <param name="workflowId">Identifier for the selected membership moderation workflow</param>
        /// <returns>View model of moderation information</returns>
        CommentModerationViewModel Get(string workflowId);

        /// <summary>
        /// Retrieves relevant workflow state of a member for admission to a specific community
        /// </summary>
        /// <param name="user">The user reference for the member requesting community admission</param>
        /// <param name="community">The community id for the community the user is looking to gain admission</param>
        /// <returns>The workflowitem state in moderation</returns>
        string GetCommentRequestState(string user, string community);

        /// <summary>
        /// Takes action on the specified workflow item, representing a
        /// membership request.
        /// </summary>
        /// <param name="workflowId">The id of the workflow </param>
        /// <param name="action">The moderation action to be taken</param>
        /// <param name="userId">The unique id of the user under moderation.</param>
        /// <param name="communityId">The unique id of the community to which membership has been requested.</param>
        void Moderate(string workflowId, string action, string userId, string communityId);

        /// <summary>
        /// Returns true if the specified community has a moderation workflow,
        /// false otherwise.
        /// </summary>
        /// <param name="communityId">ID of the community</param>
        /// <returns>True if the specified community has a moderation workflow, false otherwise</returns>
        bool IsModerated(string communityId);
    }
}
