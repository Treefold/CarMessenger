using System;
using System.Collections.Generic;
using System.Web;
using CarMessenger.Models;
using Microsoft.AspNet.SignalR;
using System.Text.Json;
using Microsoft.AspNet.Identity;

namespace CarMessenger
{
    public class ChatHub : Hub
    {
        private ApplicationDbContext context;

        public ChatHub()
        {
            context = new ApplicationDbContext();
        }
        public void Send (string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }

        public void JoinChat(string chatId)
        {
            Groups.Add(Context.ConnectionId, chatId);
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
            Message msg = new Message(chatId, userId, content);
            Clients.OthersInGroup(chatId).addMessage(JsonSerializer.Serialize(new SentMessage(msg, nickname, false)));
            context.Messages.Add(msg);
            context.SaveChanges();
        }
    }
}