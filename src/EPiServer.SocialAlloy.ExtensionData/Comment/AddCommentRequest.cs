using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.SocialAlloy.ExtensionData.Comment
{
    public class AddCommentRequest
    {
        public string User { get; set; }

        public bool IsImage { get; set; }

        public string Body { get; set; }

        public string CommentId { get; set; }

        public bool IsSubComment { get; set; }
        public string ProductId { get; set; }
        public int PhotoId { get; set; }
    }
}
