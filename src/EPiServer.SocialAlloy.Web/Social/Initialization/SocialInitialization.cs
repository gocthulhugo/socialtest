using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Social.Comments.Core;
using EPiServer.Social.Common;
using EPiServer.Social.Moderation.Core;
using EPiServer.SocialAlloy.Web.Business.Initialization;
using EPiServer.SocialAlloy.Web.Social.Adapters;
using EPiServer.SocialAlloy.Web.Social.Common.Exceptions;
using EPiServer.SocialAlloy.Web.Social.Repositories;
using EPiServer.SocialAlloy.Web.Social.Repositories.Moderation;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using StructureMap;
using System.Collections.Generic;
using EPiServer.SocialAlloy.Web.Social.Services;

namespace EPiServer.SocialAlloy.Web.Social.Initialization
{
    /// <summary>
    /// The SocialInitialization class initializes the IOC container mapping social component
    /// interfaces to their implementations.
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(DependencyResolverInitialization))]
    public class SocialInitialization : IConfigurableModule
    {
        /// <summary>
        /// Configure the IoC container before initialization.
        /// </summary>
        /// <param name="context">The context on which the container can be accessed.</param>
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Container.Configure(ConfigureContainer);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="context">The instance context.</param>
        public void Initialize(InitializationEngine context)
        {

            
            // Confirm or Create Workflow for Comments
            var workflowService = ServiceLocator.Current.GetInstance<IWorkflowService>();

            var criteria = new Criteria<WorkflowFilter>
            {
                Filter = new WorkflowFilter
                {
                    Name = "Comments Workflow"
                }
            };

            ResultPage<Workflow> pageOfWorkflows = workflowService.Get(criteria);

            int workflowCount = 0;

            foreach (Workflow wf in pageOfWorkflows.Results)
            {
                workflowCount++;
            }

            if(workflowCount > 0)
            {
                return;
            }

            var workflowTransitions = new List<WorkflowTransition>
            {
                new WorkflowTransition(new WorkflowState("Pending"),  new WorkflowState("Approved"), new WorkflowAction("Approve")),
                new WorkflowTransition(new WorkflowState("Pending"),  new WorkflowState("Rejected"), new WorkflowAction("Reject"))
            };

            Workflow commentWorkflow = new Workflow("Comments Workflow",
                workflowTransitions,
                new WorkflowState("Pending")
            );

            if (commentWorkflow != null)
            {
                try
                {
                    workflowService.Add(commentWorkflow);
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


            // Things for deleting my Workflows... gotta comment out the "return" above.

            //var criteriaaa = new Criteria<WorkflowFilter>
            //{
            //    Filter = new WorkflowFilter
            //    {
            //        Name = "Comments Workflow"
            //    }
            //};

            //ResultPage<Workflow> pageOfWorkflowsss = workflowService.Get(criteriaaa);

            //foreach (Workflow wf in pageOfWorkflowsss.Results)
            //{
            //    workflowService.Remove(wf.Id);
            //}

            
            
        }

        /// <summary>
        /// Uninitializes this instance.
        /// </summary>
        /// <param name="context">The instance context.</param>
        public void Uninitialize(InitializationEngine context)
        {
        }

        /// <summary>
        /// Configure the IOC container.
        /// </summary>
        /// <param name="configuration">The IOC container configuration.</param>
        private static void ConfigureContainer(ConfigurationExpression configuration)
        {
            configuration.For<IUserRepository>().Use(() => CreateUserRepository());
            configuration.For<IPageRepository>().Use<PageRepository>();
            configuration.For<IPageCommentRepository>().Use<PageCommentRepository>();
            configuration.For<IPageRatingRepository>().Use<PageRatingRepository>();
            configuration.For<IPageSubscriptionRepository>().Use<PageSubscriptionRepository>();
            configuration.For<ICommunityActivityAdapter>().Use<CommunityActivityAdapter>();
            configuration.For<ICommunityFeedRepository>().Use<CommunityFeedRepository>();
            configuration.For<ICommunityActivityRepository>().Use<CommunityActivityRepository>();
            configuration.For<ICommunityRepository>().Use<CommunityRepository>();
            configuration.For<ICommunityMemberRepository>().Use<CommunityMemberRepository>();
            configuration.For<ICommunityMembershipModerationRepository>().Use<CommunityMembershipModerationRepository>();
            configuration.For<IReviewService>().Use<ReviewService>();
        }

        /// <summary>
        /// Create a UserRepository.
        /// </summary>
        /// <returns>The created UserRepository instance.</returns>
        private static IUserRepository CreateUserRepository()
        {
            return new UserRepository(new UserManager<IdentityUser>(
                    new UserStore<IdentityUser>(new ApplicationDbContext<IdentityUser>("EPiServerDB")))
            );
        }
    }
}