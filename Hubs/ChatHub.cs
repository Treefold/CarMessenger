using System;
using System.Collections.Generic;
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
        private static string carGroupPrefix = "Car_";
        private static string userPrefix = "User_";
        private static IHubContext chatHub = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

        public ChatHub() {}

        public static void DeleteChat(string chatId)
        {
            // only the server can call this function
            // not verified, already trusted
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

        public void DisconnectChat(string chatId)
        {
            // only the client should call this function
            // calledback function after removing chats so that further messages won't be received
            try
            {
                // no validation needed while disconnectiong from the groups

                Groups.Remove(Context.ConnectionId, chatGroupPrefix + chatId);
            }
            catch
            {
                // do nothing
            }
        }

        public void DisconnectCar(string carId)
        {
            // only the client should call this function
            // calledback function after removing car so that further new chats or updates won't be received
            try
            {
                // no validation needed while disconnectiong from the groups

                Groups.Remove(Context.ConnectionId, carGroupPrefix + carId);
            }
            catch
            {
                // do nothing
            }
        }

        private void JoinMyChatTrusted(string chatId)
        {
            // only this class can call this function
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
            // only the client should call this function
            try
            {
                // this will be verified, not trusted

                // user validation
                string userId = Context?.User?.Identity?.GetUserId(); // this might be enough not to test the database
                if (String.IsNullOrEmpty(userId))
                {
                    return; // invalid attempt - unknown user
                }

                if (Chat.HasUser(contextdb, userId, chatId))
                {
                    // all validations passed => it's safe to add it
                    Groups.Add(Context.ConnectionId, chatGroupPrefix + chatId);
                }
            }
            catch
            {
                // do nothing
            }
        }

        public void JoinMyChats()
        {
            // only the client should call this function
            try
            {
                // this will be verified, not trusted

                // user validation
                string userId = Context?.User?.Identity?.GetUserId(); // this might be enough not to test the database
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
               
        private void JoinMyCarTrusted(string carId)
        {
            // only this class can call this function
            try
            {
                // this will not be verified, already trusted (the car exists and the user owns it)
                Groups.Add(Context.ConnectionId, carGroupPrefix + carId);
            }
            catch
            {
                // do nothing
            }
        }

        public void JoinMyCars()
        {
            // only the client should call this function
            try
            {
                // this will be verified, not trusted

                // user validation
                string userId = Context?.User?.Identity?.GetUserId(); // this might be enough not to test the database
                if (String.IsNullOrEmpty(userId))
                {
                    return; // invalid attempt - unknown user
                }

                // add car
                contextdb.Owners.Where(o => o.UserId == userId)
                    .ToList().ForEach(owner => {
                        if (owner.HasExpired())
                        {
                            owner.Delete(contextdb);
                        }
                        else
                        {
                            if (owner.Owns())
                            {
                                JoinMyCarTrusted(owner.CarId);
                            }
                        }
                    });
            }
            catch
            {
                // do nothing
            }
        }

        public void JoinMyNotifications() 
        {
            // user joins it's own notification 

            // only the client should call this function
            try
            {
                // this will be verified, not trusted

                // user validation
                string userId = Context?.User?.Identity?.GetUserId(); // this might be enough not to test the database
                if (String.IsNullOrEmpty(userId))
                {
                    return; // invalid attempt - unknown user
                }

                Groups.Add(Context.ConnectionId, userPrefix + userId);
            }
            catch
            {
                // do nothing
            }
        }

        public static void NotifyNewOwner(string userId, CarModel car, bool isCoOwner = false)
        {
            // only the server can call this function
            // not verified, already trusted
            try
            {
                if (chatHub != null)
                {
                    var carName = "(" + car.CountryCode + ") " + car.Plate;
                    chatHub.Clients.Group(userPrefix + userId).NotifyNewOwner(carName, isCoOwner);
                }
            }
            catch
            {
                // do nothing
            }
        
        }

        public static void NewChatForOwners(string carId, ChatHead head)
        {
            // only the server can call this function
            // no validations, already trusted
            try
            {
                // this notify the owners of their new chat
                chatHub.Clients.Group(carGroupPrefix + carId).AddChat(head);
            }
            catch
            {
                // do nothing
            }
        }
        
        public static void NewChatForUser(string userId, ChatHead head)
        {
            // only the server can call this function
            // no validations, already trusted
            try
            {
                // this notify the owners of their new chat
                chatHub.Clients.Group(userPrefix + userId).AddChat(head);
            }
            catch
            {
                // do nothing
            }
        }

        public static void DeleteChatForUser(string chatId, string userId)
        {
            // only the server can call this function
            // no validations, already trusted
            try
            {
                // this notify the user of its chat deletion
                chatHub.Clients.Group(userPrefix + userId).DeleteChat(chatId);
            }
            catch
            {
                // do nothing
            }
        }

        public static void DeleteCarForUser(string carId, string userId)
        {
            // only the server can call this function
            // no validations, already trusted
            try
            {
                // this notify the user of its car removal
                chatHub.Clients.Group(userPrefix + userId).DeleteCar(carId);
            }
            catch
            {
                // do nothing
            }
        }

        public void NewSeen(string chatId, string messageId)
        { 
            // only the client should call this function
            try
            {
                // this will be verified, not trusted

                // user validation
                string userId = Context?.User?.Identity?.GetUserId(); // this might be enough not to test the database
                if (String.IsNullOrEmpty(userId))
                {
                    return; // invalid attempt - unknown user
                }

                LastSeen lastSeen = contextdb.LastSeens.FirstOrDefault(s => s.userId == userId && s.chatId == chatId);
                if (lastSeen == null)
                {
                    return; // invalid attempt - unknow chat
                }

                Message msg = contextdb.Messages.Find(messageId);
                if (msg == null || msg.chatId != chatId)
                {
                    return; // invalid attempt - inexistent message in chat
                }

                /*
                 * here should be a validation that this is the last messaga in chat
                 * but we don't really care and that would be time/resourece consiuming, 
                 * the user is allowed to play with this if he really wants to (and is able to).
                 * only he can see these and it doesn't influence anybody else => it's not a vunlnerability
                 */

                lastSeen.messageId = messageId;
                contextdb.SaveChanges();
            }
            catch
            {
                // do nothing
            }
        }

        public void MessageChat(string chatId, string content)
        {
            // only the client should call this function
            try
            {
                // this will be verified, not trusted

                // user validation
                string userId = Context?.User?.Identity?.GetUserId(); // this might be enough not to test the database
                if (String.IsNullOrEmpty(userId))
                {
                    return; // invalid attempt - unknown user
                }

                if (!Chat.HasUser(contextdb, userId, chatId))
                {
                    return;  // invalid attempt - user not in chat
                }

                Message msg = new Message(chatId, userId, content);
                contextdb.Messages.Add(msg);
                contextdb.SaveChanges();

                // update last seen for the current user in the current chat
                LastSeen lastSeen = contextdb.LastSeens.FirstOrDefault(s => s.chatId == chatId && s.userId == userId);
                if (lastSeen != null)
                {
                    lastSeen.messageId = msg.Id;
                }
                contextdb.SaveChanges();

                Clients.OthersInGroup(chatGroupPrefix + chatId).addMessage(JsonSerializer.Serialize(new SentMessage(msg, Context.User.Identity.GetNickname(), false)));
            }
            catch
            {
                // do nothing
            }
        }

        public static void UpdateCarChat(string chatId, string plate, string code)
        {
            // only the server can call this function
            // not verified, already trusted
            try
            {
                if (chatHub != null)
                {
                    chatHub.Clients.Group(chatGroupPrefix + chatId).UpdateCarChat(chatId, plate, code);
                }
            }
            catch
            {
                // do nothing
            }
        }

        public static void UpdateNickChat(string chatId, string nick)
        {
            // only the server can call this function
            // not verified, already trusted
            try
            {
                if (chatHub != null)
                {
                    chatHub.Clients.Group(chatGroupPrefix + chatId).UpdateNickChat(chatId, nick);
                }
            }
            catch
            {
                // do nothing
            }
        }
        public static void UpdateNickMsg(string chatId, string nick, List<string> msgs)
        {
            // only the server can call this function
            // not verified, already trusted
            try
            {
                if (chatHub != null)
                {
                    chatHub.Clients.Group(chatGroupPrefix + chatId).UpdateNickMsg(chatId, nick, msgs);
                }
            }
            catch
            {
                // do nothing
            }
        }
    }
}