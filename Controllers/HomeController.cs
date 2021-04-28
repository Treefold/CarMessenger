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

namespace CarMessenger.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext context;

        public HomeController()
        {
            context = new ApplicationDbContext();
        }

        private static (string Plate, string CountryCode) CarTuple (CarModel car)
        {
            return (car.Plate, car.CountryCode);
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userID      = User.Identity.GetUserId();
                var userMail    = User.Identity.GetUserName();
                var ownedCarIds = context.Owners.Where(o => o.UserId == userID && (o.Category == "Owner" || o.Category == "CoOwner")).Select(o => o.CarId);

                var userChats   = context.Chats.Where(c => c.userId == userID || ownedCarIds.Contains(c.carId)).ToList();
                var allCarsIds  = userChats.Select(c => c.carId).ToList();
                var carDetails  = context.Cars.Where(c => allCarsIds.Contains(c.Id)).ToList();

                var chatsDetails = userChats.Join(
                        carDetails,
                        chat => chat.carId,
                        car  => car.Id,
                        (chat, car) => (chatId: chat.Id, owning: chat.userId != userID, plate: car.Plate, code: car.CountryCode)
                        //{
                        //    chatId = chat.Id,
                        //    owning = chat.userId != userID,
                        //    plate  = car.Plate,
                        //    code   = car.CountryCode,
                        //}
                     ).ToList();


                var chats = new Dictionary<(string chatId, bool owning, string plate, string code), List<SentMessage>>();

                foreach (var currChat in chatsDetails)
                {
                    var rawChatMessages = context.Messages.Where(m => m.chatId == currChat.chatId);
                    // TODO: Delete expired messages

                    var finalChatMessages = rawChatMessages.Join(
                            context.Users,
                            msg => msg.userId,
                            user => user.Id,
                            (msg, user) => new SentMessage // (msg, user.Nickname)
                            {
                                chatId = msg.chatId,
                                content = msg.content,
                                sendTime = msg.sendTime,
                                expiry = msg.expiry,
                                nickname = user.Nickname,
                                owned = msg.userId == userID
                            }
                        ).OrderByDescending(m => m.sendTime).ToList();

                    chats.Add(currChat, finalChatMessages);
                }

                ViewBag.chats = chats.OrderByDescending(d => d.Value.Count > 0 ? d.Value[0].sendTime : DateTime.MinValue).ToList();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        // Get: /Home/NewChat
        [HttpGet]
        public ActionResult NewChat ()
        {
            Console.WriteLine(context.Cars);
            return View();
        }

        // Post: /Home/NewChat
        [HttpPost]
        public ActionResult NewChat(NewChat newChat)
        {
            try
            {
                string userId = User.Identity.GetUserId();

                string carId = context.Cars.FirstOrDefault(c => c.Plate == newChat.carPlate && c.CountryCode == newChat.carCountryCode)?.Id;
                if (carId == null)
                {
                    ViewData["WarningMsgs"] = new List<string> { "We coudn't find that car" };
                    return View(newChat);
                }

                Chat chat = context.Chats.FirstOrDefault(c => c.carId == carId); // && c.userId == userId);
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

                    context.Chats.Add(new Chat(userId, carId));
                    context.SaveChanges();
                }
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                ViewData["DangerMsgs"] = new List<string> { e.Message };
                return View(newChat);
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