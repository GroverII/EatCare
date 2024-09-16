using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_maui.Modules
{
    [Table("food_types")]
    public class FoodType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("type_name")]
        public string TypeName { get; set; }

        public virtual ICollection<FoodItem> FoodItems { get; set; }
    }
}
