using System;

namespace EPiServer.SocialAlloy.Web.Social.Models
{
    /// <summary>
    /// The PageComment class describes a comment model used by the SocialAlloy site.
    /// </summary>
    public class PageComment
    {
        /// <summary>
        /// The comment author identifier.
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// The comment author username.
        /// </summary>
        public string AuthorUsername { get; set; }

        /// <summary>
        /// The comment body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The reference to the target the comment applies to.
        /// </summary>
        public string PageTarget { get; set; }

        /// <summary>
        /// The date/time the comment was created at.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Boolean for whether or not this is an Image comment.
        /// </summary>
        public bool IsImage { get; set; }

        public bool IsSubComment { get; set; }

        public string Parent { get; set; }

        public string CommentId { get; set; }

        public string ProductId { get; set; }
        public int PhotoId { get; set; }
    }
}