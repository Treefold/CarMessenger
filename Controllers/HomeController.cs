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
                
                var otherMessages = context.Messages.Where(m => m.personMail == userMail).OrderByDescending(m => m.sendTime).ToList();
                foreach(var msg in otherMessages)
                {
                    if (carsMessages.ContainsKey((false, msg.carPlate, msg.carCountryCode)))
                    {
                        carsMessages[(false, msg.carPlate, msg.carCountryCode)].Add(msg);
                    }
                    else
                    {
                        carsMessages.Add((false, msg.carPlate, msg.carCountryCode), new List<Message> { msg });
                    }
                }

                ViewBag.messages = carsMessages.OrderByDescending(d => d.Value.Count > 0 ? d.Value[0].sendTime : DateTime.MinValue);
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}