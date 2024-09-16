using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EatCareService.Modules
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("idusers")]
        public int Id { get; set; }

        [Required]
        [Column("username")]
        public required string Username { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("type")]
        public int Type { get; set; }

        [Required]
        public List<UserIntolerance> UserIntolerances { get; set; } = new List<UserIntolerance>();
    }
}
