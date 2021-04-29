using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CarMessenger.Models
{
    public class Message
    {
        [Key]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        [Required]
        public string chatId { get; set; }

        [Required]
        public string userId { get; set; }

        [Required]
        public string content { get; set; }

        [Required]
        public DateTime sendTime { get; private set; } = DateTime.Now;

        [Required]
        public DateTime expiry { get; private set; } = DateTime.Now.AddDays(2);

        public Message()
        {
        }

        public Message(string chatId, string userId, string content)
        {
            this.chatId = chatId;
            this.userId = userId;
            this.content = content;
        }
    }
    public class SentMessage
    {
        public string Id { get; set; }

        public string chatId { get; set; }

        public string nickname { get; set; }

        public bool owned { get; set; }

        public string content { get; set; }

        public DateTime sendTime { get; set; } = DateTime.Now;

        public DateTime expiry { get; set; } = DateTime.Now.AddDays(2);

        public SentMessage()
        {
        }

        public SentMessage(string Id, string chatId, string nickname, bool owned, string content, DateTime sendTime, DateTime expiry)
        {
            this.Id = Id;
            this.chatId = chatId;
            this.nickname = nickname;
            this.owned = owned;
            this.content = content;
            this.sendTime = sendTime;
            this.expiry = expiry;
        }

        public SentMessage(Message msg)
        {
            this.Id = msg.Id;
            this.chatId = msg.chatId;
            this.content = msg.content;
            this.sendTime = msg.sendTime;
            this.expiry = msg.expiry;
        }

        public SentMessage(Message msg, string nickname, bool owned) : this(msg)
        {
            this.nickname = nickname;
            this.owned = owned;
        }
    }
}