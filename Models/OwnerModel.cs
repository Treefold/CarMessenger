using CarMessenger.Hubs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarMessenger.Models
{
    public class OwnerModel // Owners
    {

        [Key]
        [Column(Order = 1)]
        [Required]
        [StringLength(128, ErrorMessage = "GUID excedeed length limit")]
        public string UserId { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        [StringLength(40, ErrorMessage = "GUID excedeed length limit")]
        public string CarId { get; set; }
        public CarModel car { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Owner Category excedeed length limit")]
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
        public bool HasExpired()
        {
            return DateTime.Now >= this.Expiry;
        }

        public bool IsOwner()
        {
            return this.Category == "Owner";
        }
        public bool IsCoOwner()
        {
            return this.Category == "CoOwner";
        }
        public bool IsInvited()
        {
            return this.Category == "Invited";
        }
        public bool IsRequested()
        {
            return this.Category == "Requested";
        }
        public bool Owns()
        {
            return (this.IsOwner() || this.IsCoOwner());
        }

        public void Delete(ApplicationDbContext context, bool notifyMsg = true)
        {
            if (notifyMsg)
            {
                if (this.Owns())
                {
                    var chats = context.Chats.Where(c => c.carId == this.CarId).ToList();
                    var chatIds = chats.Select(c => c.Id).ToList();
                    var lastSeens = context.LastSeens.Where(s => s.userId == this.UserId && chatIds.Contains(s.chatId));
                    context.LastSeens.RemoveRange(lastSeens);
                    // notify the user
                    chats.ForEach(chat => {
                        ChatHub.DeleteCarForUser (chat.carId, this.UserId);
                        ChatHub.DeleteChatForUser(chat.Id,    this.UserId);
                    });
                }
            }
            context.Owners.Remove(this);
        }
    }
}