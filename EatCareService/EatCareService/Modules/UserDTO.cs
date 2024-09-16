using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EatCareService.Modules
{
    public class UserDTO
    {
        [Key]
        [Column("idusers")]
        public int Id { get; set; }

        [Required]
        [Column("username")]
        public string Username { get; set; }
    }
}
