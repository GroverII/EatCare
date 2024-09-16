using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test_maui.Modules
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("idusers")]
        public int Id { get; set; }

        [Required]
        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("type")]
        public int Type { get; set; }

        public virtual ICollection<UserIntolerance> UserIntolerances { get; set; }
    }
}
