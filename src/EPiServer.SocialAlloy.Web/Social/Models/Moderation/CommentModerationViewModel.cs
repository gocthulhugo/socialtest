using EPiServer.SocialAlloy.Web.Social.Models.Comments;
using EPiServer.SocialAlloy.Web.Social.Models.Groups;
using System.Collections.Generic;

namespace EPiServer.SocialAlloy.Web.Social.Models.Moderation
{
    /// <summary>
    /// The CommunityModerationViewModel describes the information necessary
    /// to present the state of membership requests under moderation
    /// within this application.
    /// </summary>
    public class CommentModerationViewModel
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CommentModerationViewModel()
        {
            this.Items = new List<CommentRequest>();
        }

        /// <summary>
        /// Gets or sets a collection of items, associated with the
        /// selected workflow, which are under moderation.
        /// </summary>
        public IEnumerable<CommentRequest> Items { get; set; }
    }
}