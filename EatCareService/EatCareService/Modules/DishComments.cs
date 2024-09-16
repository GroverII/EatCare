using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EatCareService.Modules
{
    [Table("dish_comments")]
    public class DishComments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("dish_id")] // Это поле соответствует dish_id в таблице dish_comments
        public int DishId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("parent_comment_id")]
        public int? ParentCommentId { get; set; }

        [Required]
        [Column("comment")]
        public string? CommentText { get; set; }
    }
}
