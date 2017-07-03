using EPiServer.ServiceLocation;
using EPiServer.Social.Comments.Core;
using EPiServer.Social.Common;
using EPiServer.Social.Moderation.Core;
using EPiServer.SocialAlloy.ExtensionData.Comment;
using EPiServer.SocialAlloy.Web.Social.Common.Exceptions;
using EPiServer.SocialAlloy.Web.Social.Models;
using EPiServer.SocialAlloy.Web.Social.Models.Comments;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Core;

namespace EPiServer.SocialAlloy.Web.Social.Repositories
{
    /// <summary>
    /// The PageCommentRepository class defines the operations that can be issued
    /// against the Episerver Social CommentService.
    /// </summary>
    public class PageCommentRepository : IPageCommentRepository
    {
        private readonly IUserRepository userRepository;
        private readonly ICommentService commentService;

        /// <summary>
        /// Constructor
        /// </summary>
        public PageCommentRepository(IUserRepository userRepository, ICommentService commentService)
        {
            this.userRepository = userRepository;
            this.commentService = commentService;
        }

        /// <summary>
        /// Adds a comment with the Episerver Social Framework. 
        /// </summary>
        /// <param name="comment">The comment to add.</param>
        /// <returns>The added comment.</returns>
        public PageComment Add(PageComment comment)
        {
            var newComment = AdaptPageComment(comment);
            var otherinfo = new CommentsExtension { IsImage = comment.IsImage, IsSubComment = comment.IsSubComment, PhotoId = comment.PhotoId, ProductId = comment.ProductId};
            Composite<Comment,CommentsExtension> addedComment = null;

            try
            {
                addedComment = this.commentService.Add(newComment, otherinfo);

                if (addedComment == null)
                {
                    throw new SocialRepositoryException("The newly posted comment could not be added. Please try again");
                }
                else
                {
                    // Add WorkflowItem for Comment Approval
                    var commentid = addedComment.Data.Id;

                    // Get the Comments Workflow

                    var workflowService = ServiceLocator.Current.GetInstance<IWorkflowService>();

                    var criteria = new Criteria<WorkflowFilter>
                    {
                        Filter = new WorkflowFilter
                        {
                            Name = "Comments Workflow"
                        }
                    };

                   var pageOfWorkflows = workflowService.Get(criteria);

                    var commentWorkflow = pageOfWorkflows.Results.First<Workflow>();

                    // Build the WorkflowItem
                    var reftoComment = Reference.Create(addedComment.Data.Id.ToString());
                    
                    // Get the WorkflowItem Service and add it.
                    var workflowItemService = ServiceLocator.Current.GetInstance<IWorkflowItemService>();
                    TransitionSessionToken sessionToken = null;

                    try
                    {
                        sessionToken = workflowService.BeginTransitionSession(commentWorkflow.Id, reftoComment);

                        var workflowItem = new WorkflowItem(
                            commentWorkflow.Id,
                            commentWorkflow.InitialState,
                            reftoComment
                        );

                        var userUri = "";
                        userUri = !string.IsNullOrWhiteSpace(comment.AuthorUsername) ? 
                            userRepository.CreateAnonymousUri(comment.AuthorUsername) : 
                            userRepository.CreateAuthenticatedUri(comment.AuthorUsername);


                        var itemExtensions = new AddCommentRequest
                        {
                            Body = addedComment.Data.Body,
                            IsImage = addedComment.Extension.IsImage,
                            User = userUri,
                            PhotoId = addedComment.Extension.PhotoId,
                            ProductId = addedComment.Extension.ProductId,
                            CommentId = addedComment.Data.Id.ToString(),
                            IsSubComment = addedComment.Extension.IsSubComment
                        };

                        workflowItemService.Add(workflowItem, itemExtensions, sessionToken);
                    }
                    catch (TransitionSessionDeniedException)
                    {
                        // The workflow has indicated that the intended action
                        // is not possible given the target's current state. Handle
                        // the exception to inform the user, etc.
                    }
                    finally
                    {
                        if (sessionToken != null)
                        {
                            workflowService.EndTransitionSession(sessionToken);
                        }
                    }
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

            return AdaptComment(addedComment);
        }

        /// <summary>
        /// Gets comments from the Episerver Social Framework.
        /// </summary>
        /// <param name="filter">The application comment filtering specification.</param>
        /// <returns>A list of comments.</returns>
        public IEnumerable<PageComment> Get(PageCommentFilter filter)
        {
            var comments = new List<Composite<Comment, CommentsExtension>>();
            var pageTarget = Reference.Create(filter.PageTarget);

            try
            {
                comments = this.commentService.Get<CommentsExtension>(
                    new CompositeCriteria<CommentFilter, CommentsExtension>
                    {
                        PageInfo = new PageInfo
                        {
                            PageSize = filter.PageSize
                        },
                        Filter = new CommentFilter
                        {
                            //Ancestor = pageTarget
                            //Parent = pageTarget,
                            Ancestor = Reference.Create(filter.PageTarget)
                        },
                        OrderBy = { new SortInfo(CommentSortFields.Created, false) }
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

            return AdaptComment(comments);
        }

        public ResultPage<Comment> GetAllComments(PageData pageData, int pageSize = 10)
        {
            var reference = Reference.Create(pageData.ContentGuid.ToString());
            var criteria = new Criteria<CommentFilter>()
            {
                Filter = new CommentFilter {Ancestor = reference},
                PageInfo = new PageInfo {PageSize = pageSize}
            };
            return this.commentService.Get(criteria);

        }

        /// <summary>
        /// Adapt the application PageComment to the Episerver Social Comment 
        /// </summary>
        /// <param name="comment">The application's PageComment.</param>
        /// <returns>The Episerver Social Comment.</returns>
        private Comment AdaptPageComment(PageComment comment)
        {
            return new Comment(Reference.Create(comment.Parent), Reference.Create(comment.AuthorId), comment.Body, false);
        }

        /// <summary>
        /// Adapt a Comment to PageComment.
        /// </summary>
        /// <param name="comment">The Episerver Social Comment.</param>
        /// <returns>The PageComment.</returns>
        public PageComment AdaptComment(Composite<Comment, CommentsExtension> comment)
        {
            return new PageComment
            {
                AuthorId = comment.Data.Author.ToString(),
                AuthorUsername = this.userRepository.GetUserName(comment.Data.Author.Id),
                Body = comment.Data.Body,
                Parent = comment.Data.Parent.ToString(),
                Created = comment.Data.Created,
                PhotoId = comment.Extension.PhotoId,
                ProductId = comment.Extension.ProductId,
                IsImage = comment.Extension.IsImage,
                PageTarget = comment.Data.Parent.ToString(),
                IsSubComment = comment.Extension.IsSubComment,
                CommentId = comment.Data.Id.ToString()
            };
        }

        /// <summary>
        /// Adapt a list of Episerver Social Comment to application's PageComment.
        /// </summary>
        /// <param name="comments">The list of Episerver Social Comment.</param>
        /// <returns>The list of application PageComment.</returns>
        private IEnumerable<PageComment> AdaptComment(List<Composite<Comment, CommentsExtension>> comments)
        {
            return comments.Select(c =>
                new PageComment
                {
                    AuthorId = c.Data.Author.ToString(),
                    AuthorUsername = this.userRepository.GetUserName(c.Data.Author.Id),
                    Body = c.Data.Body,
                    Parent = c.Data.Parent.ToString(),
                    Created = c.Data.Created,
                    IsImage = c.Extension.IsImage,
                    PhotoId = c.Extension.PhotoId,
                    ProductId = c.Extension.ProductId,
                    CommentId = c.Data.Id.ToString()
                }
            );
        }
    }
}