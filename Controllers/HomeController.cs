using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using CollectionExtensions;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CarMessenger.Models;
using keyType = System.Tuple<string, bool, string, string>;
using CarMessenger.Hubs;

namespace CarMessenger.Controllers
{
    public class HomeController : Controller
    {
        private static ApplicationDbContext context = ApplicationDbContext.GetApplicationDbContext();

        public HomeController()
        {
        }

        private static (string Plate, string CountryCode) CarTuple (CarModel car)
        {
            return (car.Plate, car.CountryCode);
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            try 
            {
                if (User.Identity.IsAuthenticated)
                {

                    bool contextChanged = false;
                    string userId = User.Identity.GetUserId();

                    var ownedCar = context.Owners.Where(o => o.UserId == userId && (o.Category == "Owner" || o.Category == "CoOwner"));
                    var ownedCarIds = ownedCar.Select(o => o.CarId).ToList();
                    ownedCar.ToList().ForEach(owner => {
                        if (owner.HasExpired())
                        {
                            ownedCarIds.Remove(owner.CarId);
                            owner.Delete(context);
                            contextChanged = true;
                        }
                    });

                    var userChatsInitial = context.Chats.Where(c => c.userId == userId || ownedCarIds.Contains(c.carId));
                    var userChats = userChatsInitial.ToList();
                    userChatsInitial.ToList().ForEach(chat => {
                        if (chat.HasExpired())
                        {
                            chat.Delete(context);
                            userChats.Remove(chat);
                            contextChanged = true;
                        }
                    });

                    var allCarsIds = userChats.Select(c => c.carId).ToList();
                    var carDetails = context.Cars.Where(c => allCarsIds.Contains(c.Id)).ToList();

                    Dictionary<string, string> infoDict = userChats.Select(c => c.userId).Distinct().Where(c => c != null && c != userId).Join(
                            context.Users,
                            userKey => userKey,
                            users => users.Id,
                            (userKey, users) => new KeyValuePair<string, string>(userKey, users.Nickname)
                        ).ToList().ToDictionary(pair => pair.Key, pair => pair.Value);

                    List<ChatHead> chatsDetails = userChats.Join(
                            carDetails,
                            chat => chat.carId,
                            car => car.Id,
                            (chat, car) => new ChatHead 
                            {
                                chatId = chat.Id, 
                                owning = chat.userId != userId, 
                                plate = car.Plate, 
                                code = car.CountryCode, 
                                info = (string)(chat.userId == null || chat.userId == userId ? null : infoDict[chat.userId]),
                                createTime = chat.createTime
                            }
                         ).ToList();


                    var chats = new Dictionary<ChatHead, List<SentMessage>>();

                    foreach (var currChat in chatsDetails)
                    {
                        var rawChatMessages = context.Messages.Where(m => m.chatId == currChat.chatId);
                        // TODO: Delete expired messages

                        List<SentMessage> finalChatMessages = rawChatMessages.Join(
                                context.Users,
                                msg => msg.userId,
                                user => user.Id,
                                (msg, user) => new SentMessage 
                                {
                                    Id = msg.Id,
                                    chatId = msg.chatId,
                                    content = msg.content,
                                    sendTime = msg.sendTime,
                                    expiry = msg.expiry,
                                    nickname = user.Nickname,
                                    owned = msg.userId == userId
                                }
                            ).OrderByDescending(m => m.sendTime).ThenBy(c => c.Id).ToList();

                        string seenMsgId = null;
                        if (context.LastSeens.Any(o => true))
                        {
                            try
                            {
                                seenMsgId = context.LastSeens.First(s => s.chatId == currChat.chatId && s.userId == userId)?.messageId;
                            }
                            catch
                            {
                                seenMsgId = null;
                            }
                        }

                        if (String.IsNullOrEmpty(seenMsgId))
                        {
                            currChat.newMsgs = finalChatMessages.Count;
                        }
                        else
                        {
                            currChat.newMsgs = finalChatMessages.FindIndex(msg => msg.Id == seenMsgId);
                        }

                        chats.Add(currChat, finalChatMessages);
                    }

                    if (contextChanged)
                    {
                        context.SaveChanges();
                    }

                    if (TempData["DangerMsgs"] != null)
                        ViewBag.DangerMsgs = (List<string>)TempData["DangerMsgs"];
                    if (TempData["WarningMsgs"] != null)
                        ViewBag.WarningMsgs = (List<string>)TempData["WarningMsgs"];
                    if (TempData["SuccessMsgs"] != null)
                        ViewBag.SuccessMsgs = (List<string>)TempData["SuccessMsgs"];
                    if (TempData["InfoMsgs"] != null)
                        ViewBag.InfoMsgs = (List<string>)TempData["InfoMsgs"];

                    ViewBag.chats = chats.OrderByDescending(d => d.Value.Count > 0 ? d.Value[0].sendTime : d.Key.createTime).ToList();
                    return View();
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // Get: /Home/NewChat
        [HttpGet]
        [Authorize]
        public ActionResult NewChat ()
        {
            return View();
        }

        // Post: /Home/NewChat
        [HttpPost]
        [Authorize]
        public ActionResult NewChat(NewChat newChat)
        {
            try
            {
                CarModel car = context.Cars.FirstOrDefault(c => c.Plate == newChat.carPlate && c.CountryCode == newChat.carCountryCode);
                string carId = car?.Id;
                string userId = User.Identity.GetUserId();

                if (carId == null)
                {
                    ViewData["WarningMsgs"] = new List<string> { "We coudn't find that car" };
                    return View(newChat);
                }

                Chat chat = context.Chats.FirstOrDefault(c => c.carId == carId && c.userId == userId);
                if (chat != null)
                {
                    ViewData["WarningMsgs"] = new List<string> { "This chat already exists. If you cannot find it, please contact us for technical support!" };
                    return View(newChat);
                }

                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == carId);
                if (owner?.Category == "Owner")
                {
                    ViewData["WarningMsgs"] = new List<string> { "You are the Owner of the car. If you cannot find your car chat, please contact us for technical support!" };
                    return View(newChat);
                } else if (owner?.Category == "CoOwner")
                {
                    ViewData["WarningMsgs"] = new List<string> { "You are the CoOwner of the car. If you cannot find your car chat, please contact us for technical support!" };
                    return View(newChat);
                } else
                {
                    /*ViewBag["DangerMsgs"] = new List<string> { "Your relationship with that car is Unknown. Please contact us for technical support!" };
                    return View(msg);*/

                    Chat ch = new Chat(userId, carId);
                    context.Chats.Add(ch);
                    context.LastSeens.Add(new LastSeen (userId, ch.Id)); // add the curent user seen to the new chat
                    // add the car owners & co-owners seen to the new chat
                    context.Owners.Where(o => o.CarId == carId && (o.Category == "Owner" || o.Category == "CoOwner")).Select(o => o.UserId)
                        .ToList().ForEach(carMemeberID => context.LastSeens.Add(new LastSeen(carMemeberID, ch.Id)));

                    context.SaveChanges();
                    ChatHub.NewChatForOwners(carId,  new ChatHead(ch, car, User.Identity.GetNickname()));
                    ChatHub.NewChatForUser  (userId, new ChatHead(ch, car, null, false));
                }
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                ViewData["DangerMsgs"] = new List<string> { e.Message };
                return View(newChat);
            }
        }

        // GET: /Home/NewChatInvite/token
        [HttpGet]
        [Authorize]
        public ActionResult NewChatInvite (string token)
        {
            try
            {
                CarModel car = context.Cars.FirstOrDefault(c => c.chatInviteToken == token);
                string carId = car?.Id;
                string userId = User.Identity.GetUserId();

                if (carId == null)
                {
                    TempData["WarningMsgs"] = new List<string> { "We coudn't find that invitation. It might heve been changed." };
                    return RedirectToAction("Index", "Home");
                }

                Chat chat = context.Chats.FirstOrDefault(c => c.carId == carId && c.userId == userId); // && c.userId == userId);
                if (chat != null)
                {
                    TempData["WarningMsgs"] = new List<string> { "This chat already exists. If you cannot find it, please contact us for technical support!" };
                    return RedirectToAction("Index", "Home");
                }

                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == carId);
                if (owner?.Category == "Owner")
                {
                    TempData["WarningMsgs"] = new List<string> { "You are the Owner of the car. If you cannot find your car chat, please contact us for technical support!" };
                    return RedirectToAction("Index", "Home");
                }
                else if (owner?.Category == "CoOwner")
                {
                    TempData["WarningMsgs"] = new List<string> { "You are the CoOwner of the car. If you cannot find your car chat, please contact us for technical support!" };
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    /*ViewBag["DangerMsgs"] = new List<string> { "Your relationship with that car is Unknown. Please contact us for technical support!" };
                    return View(msg);*/
                    Chat newChat = new Chat(userId, carId);
                    context.Chats.Add(newChat);
                    context.LastSeens.Add(new LastSeen(userId, chat.Id));// add the curent user seen to the new chat
                    // add the car owners & co-owners seen to the new chat
                    context.Owners.Where(o => o.CarId == carId && (o.Category == "Owner" || o.Category == "CoOwner")).Select(o => o.UserId)
                        .ToList().ForEach(carMemeberID => context.LastSeens.Add(new LastSeen(carMemeberID, chat.Id)));

                    context.SaveChanges();
                    ChatHub.NewChatForOwners(carId,  new ChatHead(newChat, car, User.Identity.GetNickname()));
                    ChatHub.NewChatForUser  (userId, new ChatHead(newChat, car, null, false));
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return RedirectToAction("Index", "Home");
            }
        }

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}
    }
}