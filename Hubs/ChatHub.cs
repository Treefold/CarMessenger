﻿using System;
using System.Collections.Generic;
using System.Web;
using CarMessenger.Models;
using Microsoft.AspNet.SignalR;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNet.Identity;

namespace CarMessenger.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static ApplicationDbContext contextdb = ApplicationDbContext.GetApplicationDbContext();
        private static string chatGroupPrefix = "Chat_";
        private static string carGroupPrefix  = "Car_";
        private static IHubContext chatHub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

        public ChatHub() {}
        
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

        private void JoinCarById(string carId)
        {
            Groups.Add(Context.ConnectionId, carGroupPrefix + carId);
        }

        public void JoinMyCars(string userId)
        {
            contextdb.Owners.Where(o => o.UserId == userId && (o.Category == "Owner" || o.Category == "CoOwner"))
                .Select(o => o.CarId).ToList().ForEach(carId => { JoinCarById(carId); });
        }

        public static void NewChat(string carId, ChatHead head)
        {
            chatHub.Clients.Group(carGroupPrefix + carId).AddChat(head);
        }

        public void NewSeen(string userId, string chatId, string messageId)
        {
            try
            { 
                LastSeen lastSeen = contextdb.LastSeens.FirstOrDefault(s => s.userId == userId && s.chatId == chatId);
                if (lastSeen != null)
                {
                    lastSeen.messageId = messageId;
                    contextdb.SaveChanges();
                }
            } catch
            {
                // do nothing
            }
        }

        public void MessageChat(string chatId, string userId, string nickname, string content)
        {
            ApplicationUser user = contextdb.Users.Find(userId);
            if (user == null) return;
            nickname = user.Nickname;
            Message msg = new Message(chatId, userId, content);
            contextdb.Messages.Add(msg);
            contextdb.SaveChanges();
            LastSeen lastSeen = contextdb.LastSeens.FirstOrDefault(s => s.chatId == chatId && s.userId == userId);
            if (lastSeen != null)
            {
                lastSeen.messageId = msg.Id;
            }
            contextdb.SaveChanges();

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