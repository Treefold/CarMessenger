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
    [Authorize]
    public class ChatHub : Hub
    {
        private static ApplicationDbContext contextdb = ApplicationDbContext.GetApplicationDbContext();
        private static string chatGroupPrefix = "Chat_";
        private static string carGroupPrefix  = "Car_";
        private static IHubContext chatHub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

        public ChatHub() {}

        public static void DeleteChat(string chatId)
        {
            try
            {
                if (chatHub != null)
                {
                    chatHub.Clients.Group(chatGroupPrefix + chatId).DeleteChat(chatId);
                }
            }
            catch
            {
                // do nothing
            }
        }

        private void JoinMyChatTrusted(string chatId)
        {
            try
            {
                // this will not be verified, already trusted (the chat exists and has not expired)
                Groups.Add(Context.ConnectionId, chatGroupPrefix + chatId);
            }
            catch
            {
                // do nothing
            }
        }

        public void JoinNewChat(string chatId)
        {
            try
            {
                // this will be verified, not trusted

                // user validation
                string userId = Context?.User?.Identity?.GetUserId(); // this nmight be enough not to test the database
                if (String.IsNullOrEmpty(userId))
                {
                    return; // invalid attempt - unknown user
                }

                // chat validation
                Chat chat = contextdb.Chats.Find(chatId); // might fail, but catched (it's alright)
                if (chat == null)
                {
                    return; // invalid attempt - inexistent chat
                }
                if (chat.HasExpired())
                {
                    chat.Delete(contextdb);
                    return; // invalid attempt - this chat no longer exists
                }

                if (chat.userId != userId)
                {
                    string carId = contextdb.Cars.Find(chat.carId)?.Id; // might fail, but catched (it's alright)
                    if (String.IsNullOrEmpty(carId))
                    {
                        // should never happen
                        return; // invalid attempt - inexistent car
                    }
                    OwnerModel owner = contextdb.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == carId);
                    if (owner == null)
                    {
                        return; // invalid attempt - inexistent relationship between the user and the car
                    }
                    if (owner.HasExpired())
                    {
                        owner.Delete(contextdb);
                        return; // invalid attempt - this is no longer available
                    }
                    if (owner.Category != "Owner" && owner.Category != "CoOwner")
                    {
                        return; // access denied - doesn't own the car
                    }
                    // else: owns the car => OK
                }
                // else: it's the user talking to the car => OK

                // all validations passed => it's safe to add it
                Groups.Add(Context.ConnectionId, chatGroupPrefix + chatId);
            }
            catch
            {
                // do nothing
            }
        }

        public void JoinMyChats()
        {
            try
            {
                // this will be verified, not trusted

                // user validation
                string userId = Context?.User?.Identity?.GetUserId(); // this nmight be enough not to test the database
                if (String.IsNullOrEmpty(userId))
                {
                    return; // invalid attempt - unknown user
                }

                // add to non-owner chats
                contextdb.Chats.Where(c => c.userId == userId).Select( c => c.Id) // get all non-owner chats
                    .ToList().ForEach(chatId => { JoinMyChatTrusted(chatId); }); // join all non-owner chats group

                // add owner chats
                contextdb.Owners.Where(o => o.UserId == userId && (o.Category == "Owner" || o.Category == "CoOwner")) // get the id of all owned cars
                    .Join( // get all owned cars
                        contextdb.Cars,
                        owner => owner.CarId,
                        car   => car.Id,
                        (owner, car) => car
                    ).Join( // get all owned chats
                        contextdb.Chats,
                        car  => car.Id,
                        chat => chat.carId,
                        (car, chat) => chat.Id
                    ).ToList().ForEach(chatId => { JoinMyChatTrusted(chatId); }); // join all owner chats group
            }
            catch
            {
                // do nothing
            }
        }
               

        private void JoinCarById(string carId)
        {
            Groups.Add(Context.ConnectionId, carGroupPrefix + carId);
        }

        public void JoinMyCars(string userId)
        {
            var user = Context.User;

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