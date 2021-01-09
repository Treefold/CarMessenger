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

        [Required]
        public string Category { get; set; } = "Owner";
        // Owner     - The first one to have the car (the ownership can be passed to a CoOwner)
        // CoOwner   - The others that use that car
        // Invited   - Invitation sent by the Owner to someone to become a CoOwner
        // Requested - Invitation sent to the Owner to become a CoOwner
        // Invitation Rejected
        // Request Denied

        public DateTime Expiry { get; set; } = DateTime.MaxValue; // never

        public OwnerModel()
        {
        }

        public OwnerModel(string userId, string carId)
        {
            UserId = userId;
            CarId = carId;
        }

        public OwnerModel(string userId, string carId, string category) : this(userId, carId)
        {
            Category = category;
        }

        public OwnerModel(string userId, string carId, string category, DateTime expiry) : this(userId, carId, category)
        {
            Expiry = expiry;
            //if (DateTime.Compare(Expiry, DateTime.Now) > 0)
            //{
            //    Expiry = expiry;
            //}
            //else
            //{
            //    Expiry = DateTime.MaxValue;
            //}
        }
    }
}