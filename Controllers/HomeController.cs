using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using CollectionExtensions;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CarMessenger.Models;

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
                var userID = User.Identity.GetUserId();
                var userMail = User.Identity.GetUserName();
                var carsMessages = new Dictionary<(bool owning, string plate, string code), List<Message>>();

                var ownedCarIds = context.Owners.Where(o => o.UserId == userID && (o.Category == "Owner" || o.Category == "CoOwner")).Select(o => o.CarId);
                var ownedCars = context.Cars.Where(c => ownedCarIds.Contains(c.Id)).ToList();
                foreach (var car in ownedCars)
                {
                    var carMessages = context.Messages.Where(m => m.carPlate == car.Plate && m.carCountryCode == car.CountryCode).OrderByDescending(m => m.sendTime).ToList();
                    carsMessages.Add((true, car.Plate, car.CountryCode), carMessages);
                }

                var otherCarIds = context.Owners.Where(o => o.UserId == userID && (o.Category == "Conversation")).Select(o => o.CarId);
                var otherdCars = context.Cars.Where(c => otherCarIds.Contains(c.Id)).ToList();
                foreach (var car in otherdCars)
                {
                    var carMessages = context.Messages.Where(m => m.carPlate == car.Plate && m.carCountryCode == car.CountryCode).OrderByDescending(m => m.sendTime).ToList();
                    carsMessages.Add((false, car.Plate, car.CountryCode), carMessages);
                }

                //var otherMessages = context.Messages.Where(m => m.personMail == userMail).OrderByDescending(m => m.sendTime).ToList();
                //foreach(var msg in otherMessages)
                //{
                //    if (carsMessages.ContainsKey((false, msg.carPlate, msg.carCountryCode)))
                //    {
                //        carsMessages[(false, msg.carPlate, msg.carCountryCode)].Add(msg);
                //    }
                //    else
                //    {
                //        carsMessages.Add((false, msg.carPlate, msg.carCountryCode), new List<Message> { msg });
                //    }
                //}

                ViewBag.carsMessages = carsMessages.OrderByDescending(d => d.Value.Count > 0 ? d.Value[0].sendTime : DateTime.MinValue).ToList();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public ActionResult NewMessage ()
        {
            return View();
        }

        // Post: Car/InviteCoOwner/id/mail
        [HttpPost]
        public ActionResult NewMessage(NewMessage msg)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                string carId = context.Cars.FirstOrDefault(c => c.Plate == msg.carPlate && c.CountryCode == msg.carCountryCode)?.Id;

                if (carId == null)
                {
                    ViewBag["WarningMsgs"] = new List<string> { "We coudn't find that car" };
                    return View(msg);
                }
                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == carId);
                if (owner?.Category == "Invited")
                {
                    ViewBag["WarningMsgs"] = new List<string> { "You are already invited to CoOwn that car", "Please accept the inviation to be automatically added to the car private group", "Or you can decline the invitation to be able to start a new conversation with the car Owners"};
                    return View(msg);
                } else if (owner?.Category == "Requested")
                {
                    ViewBag["WarningMsgs"] = new List<string> { "You are already send a request to CoOwn that car", "Please wait to be accepted so that you will be automatically added to the car private group", "Or you can remove the request to be able to start a new conversation with the car Owners" };
                    return View(msg);
                }

                if (owner == null)
                {
                    owner = new OwnerModel(userId, carId, "Conversation", DateTime.Now.AddDays(2));
                    context.Owners.Add(owner);
                    //context.Messages.Add(new Message())
                    context.SaveChanges();
                } 
                else if (owner.Category == "Owner")
                {

                } 
                else if (owner.Category == "CoOwner")
                {

                } 
                else
                {
                    ViewBag["DangerMsgs"] = new List<string> { "Your relationship with that car is Unknown. Please contact us for technical support!" };
                    return View(msg);
                }
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return Redirect("../../Manage");
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