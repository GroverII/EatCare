using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test_maui.Modules
{
    [Table("user_intolerances")]
    public class UserIntolerance
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("ingredient_id")]
        public int IngredientId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("IngredientId")]
        public virtual FoodIngredient Ingredient { get; set; }
    }
}
