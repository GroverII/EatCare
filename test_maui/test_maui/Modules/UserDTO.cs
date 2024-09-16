using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test_maui.Modules
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
