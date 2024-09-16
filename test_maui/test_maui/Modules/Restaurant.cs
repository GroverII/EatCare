using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_maui.Modules
{
    [Table("restaurants")]
    public class Restaurant
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("restaurant_name")]
        public string RestaurantName { get; set; }

        public virtual ICollection<FoodItem> FoodItems { get; set; }
    }
}
