using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CarMessenger.Models
{
    public class NewMessage
    {
        //[Key]
        //public string id { get; set; }

        [Required(ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        [RegularExpression(@"[0-9A-Z][0-9A-Z-]{3,8}[0-9A-Z]", ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        public string carPlate { get; set; }

        [Required(ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))] // from the plate
        [RegularExpression(@"[A-Z]{1,3}", ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))]
        public string carCountryCode { get; set; }
    }
    public class Message
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        public string senderEmail { get; set; } // can be null if the user is anonymous

        public string senderNickname { get; set; }// can be null if the user is anonymous

        [Required]
        public string carPlate { get; set; }

        [Required]
        public string carCountryCode { get; set; }

        public string personNickname { get; set; } // null if you own the car

        [Required]
        public bool owning { get; set; }

        [Required]
        public string content { get; set; }

        [Required]
        public DateTime sendTime { get; private set; } = DateTime.Now;

        [Required]
        public DateTime expiry { get; private set; } = DateTime.Now.AddDays(2);

        public Message()
        {
        }

        public Message(string senderEmail, string senderNickname, string carPlate, string carCountryCode, string personNickname, bool owning, string content)
        {
            this.senderEmail = senderEmail;
            this.senderNickname = senderNickname;
            this.carPlate = carPlate;
            this.carCountryCode = carCountryCode;
            this.personNickname = personNickname;
            this.owning = owning;
            this.content = content;
        }
    }
}