using CarMessenger.Hubs;
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
        [StringLength(40, ErrorMessage = "GUID excedeed length limit")]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        // [Required] // when null it is the car private group
        [Index("UniqueChat", 2, IsUnique = true)]
        [StringLength(128, ErrorMessage = "GUID excedeed length limit")]
        public string userId { get; set; }

        [Required]
        [Index("UniqueChat", 1, IsUnique = true)]
        [StringLength(40, ErrorMessage = "GUID excedeed length limit")]
        public string carId { get; set; }

        public DateTime createTime { get; set; } = DateTime.Now;

        public DateTime deleteTime { get; set; } = DateTime.Now.AddDays(2);

        public Chat()
        {
        }

        public Chat(string userId, string carId) : this()
        {
            this.userId = userId;
            this.carId = carId;
        }

        public Chat(string userId, string carId, DateTime deleteTime) : this(userId, carId)
        {
            this.deleteTime = deleteTime;
        }

        public bool HasExpired()
        {
            return DateTime.Now >= this.deleteTime;
        }

        public void Delete(ApplicationDbContext context)
        {
            ChatHub.DeleteChat(this.Id); // notify users of this chat deletion
            var seens = context.LastSeens.Where(s => s.chatId == this.Id);
            context.LastSeens.RemoveRange(seens); // get rid of all seen markers
            context.Chats.Remove(this); // remove this chat
        }

        public bool HasUser(ApplicationDbContext contextdb, string userId)
        {
            if (this.HasExpired())
            {
                this.Delete(contextdb);
                return false; // invalid attempt - this chat no longer exists
            }

            if (this.userId != userId)
            {
                string carId = contextdb.Cars.Find(this.carId)?.Id; // might fail, but catched (it's alright)
                if (String.IsNullOrEmpty(carId))
                {
                    // should never happen
                    return false; // invalid attempt - inexistent car
                }
                OwnerModel owner = contextdb.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == carId);
                if (owner == null)
                {
                    return false; // invalid attempt - inexistent relationship between the user and the car
                }
                if (owner.HasExpired())
                {
                    owner.Delete(contextdb);
                    return false; // invalid attempt - this is no longer available
                }
                if (!owner.Owns())
                {
                    return false; // access denied - doesn't own the car
                }
                // else: owns the car => OK
            }
            // else: it's the user talking to the car => OK
            return true; // OK
        }

        public static bool HasUser(ApplicationDbContext contextdb, string userId, string chatId)
        {
            // chat validation
            Chat chat = contextdb.Chats.Find(chatId); // might fail, but catched (it's alright)
            if (chat == null)
            {
                return false; // invalid attempt - inexistent chat
            }

            return chat.HasUser(contextdb, userId);
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

    public class ChatHead
    {
        public string chatId;
        public bool owning;
        public string plate;
        public string code;
        public string info;
        public DateTime createTime;
        public int newMsgs = 0;

        public ChatHead()
        {
        }

        public ChatHead(Chat chat, CarModel car, string nickname)
        {
            this.chatId = chat.Id;
            this.owning = true;
            this.plate = car.Plate;
            this.code = car.CountryCode;
            this.info = nickname;
            this.createTime = chat.createTime;
        }
    }
}