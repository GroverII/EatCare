using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EatCareService.Modules
{
    [Table("guests")]
    public class Guest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("guest_name")]
        public string GuestName { get; set; }

        [Column("ip_address")]
        public string IPAddress { get; set; }

        [Column("last_activity")]
        public DateTime LastActivity { get; set; }
    }
}
