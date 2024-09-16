using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EatCareService.Modules
{
    [Table("food_items")]
    public class FoodItem
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("item_name")]
        public string ItemName { get; set; } = string.Empty;

        [Column("restaurant_id")]
        public int RestaurantId { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("food_type_id")]
        public int FoodTypeId { get; set; }

        // Обновление навигационных свойств
        [ForeignKey("RestaurantId")]
        public virtual Restaurant Restaurant { get; set; } = new Restaurant
        {
            RestaurantName = string.Empty, // Установите значение по умолчанию
            AssetId = string.Empty // Установите значение по умолчанию
        };

        [ForeignKey("FoodTypeId")]
        public virtual FoodType FoodType { get; set; } = new FoodType();

        // Остальная часть класса остается без изменений
    }
}
