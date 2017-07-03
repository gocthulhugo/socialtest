using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SocialAlloy.Web.Models.Pages;
using EPiServer.SocialAlloy.Web.Social.Blocks;
using EPiServer.SocialAlloy.Web.Social.Models.Photos;

namespace EPiServer.SocialAlloy.Web.Social.Pages
{
    /// <summary>
    /// Used for customer photos for a product.  
    /// </summary>
    [ContentType(
        DisplayName = "PhotosPage", 
        GUID = "8b4c5048-2116-467d-9f04-9e7fd6648955", 
        Description = "Customer photos page for given product")]

    [ImageUrl(Global.StaticGraphicsFolderPath + "page-type-thumbnail-standard.png")]
    public class PhotosPage : StandardPage
    {
        /// <summary>
        /// The feed section of the photos page. Local feed block will display feed items.
        /// </summary>
        [Display(
            Name = "Feed Block",
            Description = "The feed section of the photos page. Local feed block will display feed items for the pages a user has subscriped to.",
            GroupName = SystemTabNames.Content,
            Order = 2)]
        public virtual FeedBlock Feed { get; set; }

        public virtual string SomeOtherData { get; set; }

        public virtual string ProductId { get; set; }

        [Ignore]
        public Photos ProductPhotos { get; set; }

        public PhotosPage()
        {
            ProductPhotos = new Photos();
        }

    }
}