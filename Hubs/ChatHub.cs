﻿using System;
using System.Collections.Generic;
using System.Web;
using CarMessenger.Models;
using Microsoft.AspNet.SignalR;
using System.Text.Json;
using Microsoft.AspNet.Identity;

namespace CarMessenger.Hubs
{
    public class ChatHub : Hub
    {
        private static ApplicationDbContext context = null;
        private static string chatGroupPrefix = "Chat_";
        private static ChatHub chatHub = null;

        public ChatHub()
        {
            if (context == null)
                context = new ApplicationDbContext();

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
            Message msg = new Message(chatId, userId, content);
            Clients.OthersInGroup(chatGroupPrefix + chatId).addMessage(JsonSerializer.Serialize(new SentMessage(msg, nickname, false)));
            context.Messages.Add(msg);
            context.SaveChanges();
        }

        public static void DeleteChat(string chatId)
        {
            if (chatHub != null)
                chatHub.Clients.Group(chatGroupPrefix+chatId).DeleteChat(chatId);
        }
    }
}