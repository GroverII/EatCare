using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using test_maui.Modules;

namespace test_maui.Data
{


    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Guest> Guests { get; set; }
        public DbSet<FoodIngredient> FoodIngredients { get; set; }
        public DbSet<UserIntolerance> UserIntolerances { get; set; }

        private readonly string connectionString = "Server=localhost;Database=testing_db;User=root;Password=;";//10.10.34.170, 192.168.1.84, localhost

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 28))); // Замените версию на соответствующую вашей MySQL версии
        }

    }

}
