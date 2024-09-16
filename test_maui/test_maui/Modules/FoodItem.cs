using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_maui.Modules
{
    [Table("food_items")]
    public class FoodItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("item_name")]
        public string ItemName { get; set; }

        [Column("restaurant_id")]
        public int RestaurantId { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("food_type_id")]
        public int FoodTypeId { get; set; }

        [ForeignKey("RestaurantId")]
        public virtual Restaurant Restaurant { get; set; }

        [ForeignKey("FoodTypeId")]
        public virtual FoodType FoodType { get; set; }
    }
}
