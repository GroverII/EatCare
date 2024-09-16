using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EatCareService.Modules
{
    [Table("restaurants_comments")]
    public class RestaurantsComments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("restaurants_id")]
        public int RestaurantsId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("parent_comment_id")]
        public int? ParentCommentId { get; set; }

        [Required]
        [Column("comment")]
        public string CommentText { get; set; }

        [ForeignKey("RestaurantsId")]
        public Restaurant Restaurant { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
