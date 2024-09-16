using Microsoft.EntityFrameworkCore;
using EatCareService.Modules; // Замените это на правильное пространство имен

namespace EatCareService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<FoodIngredient> FoodIngredients { get; set; }
        public DbSet<UserIntolerance> UserIntolerances { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<FoodType> FoodTypes { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<FoodItemIngredient> FoodItemIngredients { get; set; }
        public DbSet<DishRating> DishRatings { get; set; }
        public DbSet<RestaurantRating> RestaurantRatings { get; set; }
        public DbSet<RestaurantsComments> RestaurantsComments { get; set; }
        public DbSet<DishComments> DishComments { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }
}
