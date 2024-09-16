using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EatCareService.Modules
{
    [Table("restaurants_ratings")]
    public class RestaurantRating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("restaurant_id")]
        public int RestaurantId { get; set; }

        [Column("rating_value")]
        public float RatingValue { get; set; }

        [Column("date")]
        public DateTime? Date { get; set; }
    }
}
