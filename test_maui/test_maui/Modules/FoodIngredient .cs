using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_maui.Modules
{
    [Table("food_ingredients")]
    public class FoodIngredient
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("ingredient_name")]
        public string IngredientName { get; set; }

        public virtual ICollection<FoodItemIngredient> FoodItemIngredients { get; set; }
        public virtual ICollection<UserIntolerance> UserIntolerances { get; set; }
    }

}
