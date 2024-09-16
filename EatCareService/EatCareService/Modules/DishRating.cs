using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EatCareService.Modules
{
    [Table("dish_ratings")]
    public class DishRating
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("dish_id")]
        public int DishId { get; set; }

        [Column("rating_value")]
        public float RatingValue { get; set; }

        [Column("date")]
        public DateTime? Date { get; set; }
    }
}
