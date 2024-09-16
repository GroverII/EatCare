using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ApiService;

namespace test_maui.Modules
{
    public class RestaurantDisplayData
    {
            public int Id { get; set; }
            public ImageSource ImageContent { get; set; }
            public string RestaurantName { get; set; }
            public List<FoodItemData> FoodItems { get; set; }
            public double Ratings { get; set; }
    }
}
