using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EatCareService.Modules
{
    [Table("api_keys")]
    public class ApiKey
    {
        [Key]
        [Column("idapi_keys")]
        public int Id { get; set; }

        [Required]
        [Column("userID")]
        public required int UserId { get; set; }

        [Required]
        [Column("api_key")]
        public required string ApiKeyString { get; set; }

        // Define a navigation property for the foreign key relationship
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
