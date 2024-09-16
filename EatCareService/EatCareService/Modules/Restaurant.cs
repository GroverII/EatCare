using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EatCareService.Modules
{
    [Table("restaurants")]
    public class Restaurant
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("restaurant_name")]
        public required string RestaurantName { get; set; }

        [Required]
        [Column("asset_id")]
        public required string AssetId { get; set; }

        public virtual ICollection<FoodItem>? FoodItems { get; set; }
    }
}
