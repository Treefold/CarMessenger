using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarMessenger.Models
{
    public class OwnerModel
    {

        [Key]
        [Column(Order = 1)]
        [Required]
        public string UserId { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        public string CarId { get; set; }

        //[Required]
        //public string Category { get; set; } = "Owner";

        public OwnerModel(string userId, string carId)
        {
            UserId = userId;
            CarId = carId;
        }

        public OwnerModel()
        {
        }
    }
}