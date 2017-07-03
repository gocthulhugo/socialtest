using EPiServer.Core;

namespace EPiServer.SocialAlloy.Web.Social.Models
{
    /// <summary>
    /// A view model for submitting a PageComment.
    /// </summary>
    public class CommentFormViewModel
    {
        /// <summary>
        /// Default parameterless constructor required for view form submitting.
        /// </summary>
        public CommentFormViewModel()
        {
        }

        /// <summary>
        /// The comment body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the whether the block is configured to send an activity when a new comment is added. 
        /// </summary>
        public bool SendActivity { get; set; }
        
        /// <summary>
        /// Gets or sets the whether the submission is an Image or not. 
        /// </summary>
        public bool IsImage { get; set; }

        /// <summary>
        /// Gets or sets the reference link of the page containing the comment form.
        /// </summary>
        public PageReference CurrentPageLink { get; set; }

        /// <summary>
        /// Gets or sets whether the submission is a subcomment or not. 
        /// </summary>
        public bool IsSubComment { get; set; }

        /// <summary>
        /// Gets or sets the parent comment, if not top level comment. 
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// Gets or sets the comment customer id
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the photo parent of the comment
        /// </summary>
        public int PhotoId { get; set; }
        
        /// <summary>
        /// Gets or sets the product parent of the photo parent of the comment.
        /// </summary>
        public string ProductId { get; set; }
    }
}