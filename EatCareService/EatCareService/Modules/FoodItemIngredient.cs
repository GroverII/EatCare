using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatCareService.Modules
{
    [Table("food_item_ingredients")]
    public class FoodItemIngredient
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("food_item_id")]
        public int FoodItemId { get; set; }

        [Column("ingredient_id")]
        public int IngredientId { get; set; }

        [ForeignKey("FoodItemId")]
        public virtual FoodItem FoodItem { get; set; }

        [ForeignKey("IngredientId")]
        public virtual FoodIngredient Ingredient { get; set; }
    }
}
