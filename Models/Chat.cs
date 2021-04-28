using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarMessenger.Models
{
    public class Chat
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        // [Required] // when null it is the car private group
        [Index("UniqueChat", 2, IsUnique = true)]
        public string userId { get; set; }

        [Required]
        [Index("UniqueChat", 1, IsUnique = true)]
        public string carId { get; set; }

        public Chat()
        {
        }

        public Chat(string userId, string carId)
        {
            this.userId = userId;
            this.carId = carId;
        }
    }

    public class NewChat
    {
        [Required(ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        [RegularExpression(@"[0-9A-Z][0-9A-Z-]{3,8}[0-9A-Z]", ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        [Display(Name = "Plate")]
        public string carPlate { get; set; }

        [Required(ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))] // from the plate
        [RegularExpression(@"[A-Z]{1,3}", ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))]
        [Display(Name = "CountryCode")]
        public string carCountryCode { get; set; }
    }
}