using System;
using System.Collections.Generic;
using System.Web;
using CarMessenger.Models;
using Microsoft.AspNet.SignalR;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNet.Identity;

namespace CarMessenger.Hubs
{
    public class ChatHub : Hub
    {
        private static ApplicationDbContext context = ApplicationDbContext.GetApplicationDbContext();
        private static string chatGroupPrefix = "Chat_";
        private static ChatHub chatHub = null;

        public ChatHub()
        {
            //if (context == null)
            //    context = ApplicationDbContext.GetApplicationDbContext();

            if (chatHub == null)
                chatHub = this;
        }
        
        public void Send (string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }

        public void JoinChat(string chatId)
        {
            Groups.Add(Context.ConnectionId, chatGroupPrefix + chatId);
        }
        
        public void JoinChats(List<string> chatIdList)
        {
            foreach (var chatId in chatIdList)
            {
                JoinChat(chatId);
            }
        }

        public void MessageChat(string chatId, string userId, string nickname, string content)
        {
            ApplicationUser user = context.Users.Find(userId);
            if (user == null) return;
            nickname = user.Nickname;
            Message msg = new Message(chatId, userId, content);
            context.Messages.Add(msg);
            context.SaveChanges();

            Clients.OthersInGroup(chatGroupPrefix + chatId).addMessage(JsonSerializer.Serialize(new SentMessage(msg, nickname, false)));
        }

        public static void DeleteChat(string chatId)
        {
            if (chatHub != null)
                chatHub.Clients.Group(chatGroupPrefix+chatId).DeleteChat(chatId);
        }

        public static void UpdateCarChat(string chatId, string plate, string code)
        {
            if (chatHub != null)
                chatHub.Clients.Group(chatGroupPrefix + chatId).UpdateCarChat(chatId, plate, code);
        }

        public static void UpdateNickChat(string chatId, string nick)
        {
            if (chatHub != null)
                chatHub.Clients.Group(chatGroupPrefix + chatId).UpdateNickChat(chatId, nick);
        }
        public static void UpdateNickMsg(string chatId, string nick, List<string> msgs)
        {
            if (chatHub != null)
                chatHub.Clients.Group(chatGroupPrefix + chatId).UpdateNickMsg(chatId, nick, msgs);
        }
    }
}