namespace EatCareService.Modules
{
    public class DishCommentDTO
    {
        public int DishId { get; set; }
        public int UserId { get; set; }
        public int ParentCommentId { get; set; }
        public string CommentText { get; set; }
    }
}
