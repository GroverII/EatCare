namespace EatCareService.Modules
{
    public class CommentDTO
    {
        public int RestaurantsId { get; set; }
        public int UserId { get; set; }
        public int ParentCommentId { get; set; }
        public string CommentText { get; set; }
    }
}
