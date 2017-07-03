using System.Collections.Generic;

namespace EPiServer.SocialAlloy.Web.Social.ViewModels
{
    public class ReviewsViewModel
    {
        public ReviewsViewModel()
        {
            this.Reviews = new List<ReviewViewModel>();
            this.Statistics = new ReviewStatisticsViewModel();
        }
        
        public ReviewsViewModel(string productId, string productName)
        {
            this.ProductId = productId;
            this.ProductName = productName;
            this.Reviews = new List<ReviewViewModel>();
            this.Statistics = new ReviewStatisticsViewModel();
        }
        public string ProductId { get; set; }
        public string ProductName { get; set; }

        public ReviewStatisticsViewModel Statistics { get; set; }        

        public List<ReviewViewModel> Reviews { get; set; }
    }
}