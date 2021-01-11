using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CarMessenger.Models
{
    public class Message
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        public string mail { get; set; } // can be null if the user is anonymous

        [Required]
        public string carPlate { get; set; }

        [Required]
        public string carCountryCode { get; set; }

        [Required]
        public DateTime sendTime { get; private set; } = DateTime.Now;

        public DateTime expiry { get; private set; } = DateTime.Now.AddDays(2);
    }
}