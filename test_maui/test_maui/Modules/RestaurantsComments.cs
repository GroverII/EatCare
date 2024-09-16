using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_maui.Modules
{
    public class RestaurantsComments
    {
        public int Id { get; set; }
        public int RestaurantsId { get; set; }
        public int UserId { get; set; }
        public int? ParentCommentId { get; set; }
        public string CommentText { get; set; }
    }
}
