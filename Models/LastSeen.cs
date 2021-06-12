using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarMessenger.Models
{
    public class LastSeen
    {

        [Key]
        [StringLength(40, ErrorMessage = "GUID excedeed length limit")]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(128, ErrorMessage = "User GUID excedeed length limit")]
        public string userId { get; set; }


        [Required]
        [StringLength(40, ErrorMessage = "Chat GUID excedeed length limit")]
        public string chatId { get; set; }


        //NOT [Required]
        [StringLength(40, ErrorMessage = "Message GUID excedeed length limit")]
        public string messageId { get; set; }

        public LastSeen()
        {
        }

        public LastSeen(string userId, string chatId, string messageId = null)
        {
            this.userId = userId;
            this.chatId = chatId;
            this.messageId = messageId;
        }
    }
}