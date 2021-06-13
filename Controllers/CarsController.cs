using CarMessenger.Models;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;
using CarMessenger.Hubs;
using System.Threading.Tasks;

namespace CarMessenger.Controllers
{
    [Authorize]
    public class CarsController : Controller
    {
        private static ApplicationDbContext context = ApplicationDbContext.GetApplicationDbContext();

        public CarsController()
        {
        }

        //protected override void Dispose(bool disposing)
        //{
        //    context.Dispose();
        //    base.Dispose(disposing);
        //}

        // GET: Car
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            ViewData["cars"] = context.Cars.ToList();

            return View();
        }

        // GET: Car/Details/id
        [HttpGet]
        public ActionResult Details(string id)
        {
            string userId = User.Identity.GetUserId();
            OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);

            if (owner == null && !User.IsInRole("Admin"))
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            var car = context.Cars.Find(id);
            if (car == null)
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            if (owner != null && owner.Category == "Owner")
            {
                ViewBag.Owned = true;
                ViewBag.OwnerName = User.Identity.GetUserName();
            }
            else if (User.IsInRole("Admin") || owner.Category == "CoOwner")
            {
                ViewBag.Owned = false;
                OwnerModel realOwner = context.Owners.FirstOrDefault(o => o.CarId == id && o.Category == "Owner");
                ApplicationUser realOwnerUser = null;
                if (realOwner != null)
                    realOwnerUser = context.Users.Find(realOwner.UserId);
                if ((realOwner == null || realOwnerUser == null))
                {
                    string msg = "";
                    if (User.IsInRole("Admin"))
                    {
                        msg = "This car is not Owned";
                    }
                    else
                    {
                        owner.Category = "Owner";
                        context.SaveChanges();
                        ViewBag.Owned = true;
                        ViewBag.OwnerName = User.Identity.GetUserName();
                        msg = "This car didn't have an Owner. Now it choosed you!";
                    }
                    if (TempData["InfoMsgs"] == null)
                    {
                        TempData["InfoMsgs"] = new List<string> { msg };
                    }
                    else
                    {
                        //List<string> infoMsgs = TempData["InfoMsgs"] as List<string>;
                        //infoMsgs.Add("This car didn't have an Owner. Now it choosed you!");
                        //TempData["InfoMsgs"] = infoMsgs;
                        (TempData["InfoMsgs"] as List<string>).Add(msg);
                    }
                }
                else
                {
                    ViewBag.OwnerName = realOwnerUser.UserName;
                }
            }
            else
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            List<OwnerModel> carOwners = context.Owners.Where(o => o.CarId == id).ToList();

            try
            {
                context.Owners.RemoveRange(carOwners.Where(o => DateTime.Compare(o.Expiry, DateTime.Now) < 0));
                context.SaveChanges();

                if (TempData["DangerMsgs"] != null)
                {
                    ViewBag.DangerMsgs = TempData["DangerMsgs"];
                }
                if (TempData["WarningMsgs"] != null)
                {
                    ViewBag.WarningMsgs = TempData["WarningMsgs"];
                }
                if (TempData["SuccessMsgs"] != null)
                {
                    ViewBag.SuccessMsgs = TempData["SuccessMsgs"];
                }
                if (TempData["InfoMsgs"] != null)
                {
                    ViewBag.InfoMsgs = TempData["InfoMsgs"];
                }
            }
            catch
            {
                ViewBag.DangerMsgs = new List<string> { "Server Error: Action cancelled" };
            }

            carOwners = carOwners.Where(o => DateTime.Compare(o.Expiry, DateTime.Now) >= 0).ToList();

            List<string> coOwnersId = carOwners.Where(o => o.Category == "CoOwner").Select(o => o.UserId).ToList();
            if (coOwnersId.Count > 0)
            {
                ViewBag.CoOwners = context.Users.Where(u => coOwnersId.Contains(u.Id)).Select(u => u.UserName).ToList();
            }

            if (User.IsInRole("Admin") || owner.Category == "Owner")
            {
                List<String> pendingRequests = carOwners.Where(o => o.Category == "Requested").Select(o => o.UserId).ToList();
                if (pendingRequests.Count > 0)
                {
                    ViewBag.Requests = context.Users.Where(u => pendingRequests.Contains(u.Id)).Select(u => u.UserName).ToList();
                }

                List<String> pendingInvitations = carOwners.Where(o => o.Category == "Invited").Select(o => o.UserId).ToList();
                if (pendingInvitations.Count > 0)
                {
                    ViewBag.Invitations = context.Users.Where(u => pendingInvitations.Contains(u.Id)).Select(u => u.UserName).ToList();
                }
            }

            return View(car);
        }

        // GET: Car/ChatInvite/id
        [HttpGet]
        public ActionResult ChatInvite(string id)
        {
            if (id == null)
            {
                TempData["InfoMsgs"] = new List<string> { "No Car Id Selected" };
                return RedirectToAction("Index", "Manage");
            }

            CarModel car = null;
            if (TempData["Car"] != null && ((CarModel)TempData["Car"]).Id == id)
            {
                car = (CarModel)TempData["Car"];
                if (TempData["SuccessMsgs"] != null)
                    ViewBag.SuccessMsgs = (List<string>) TempData["SuccessMsgs"];
                if (TempData["WarningMsgs"] != null)
                    ViewBag.WarningMsgs = (List<string>) TempData["WarningMsgs"];
            }
            else
            {
                string userId = User.Identity.GetUserId();
                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);

                if (owner == null && !User.IsInRole("Admin"))
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                    return RedirectToAction("Index", "Manage");
                }

                car = context.Cars.Find(id);
                if (car == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                    return RedirectToAction("Index", "Manage");
                }
            }

            ViewBag.ChatToken = car.chatInviteToken;
            ViewBag.ChatLink = car.chatInviteLink;

            return View(car);
        }

        // GET: Car/NewChatInvite/id/token
        [HttpGet]
        public async Task<ActionResult> NewChatInvite(string id, string token)
        {
            if (id == null)
            {
                TempData["InfoMsgs"] = new List<string> { "No Car Id Selected" };
                return RedirectToAction("Index", "Manage");
            }
            string userId = User.Identity.GetUserId();
            OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);

            if (owner == null && !User.IsInRole("Admin"))
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            var car = context.Cars.Find(id);
            if (car == null)
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            if (car.chatInviteToken == token)
            {
                await car.GenerateNewChatInviteToken();
                context.SaveChanges();
                TempData["SuccessMsgs"] = new List<string> { "Chat Invite Changed" };
            }
            else
            {
                TempData["WarningMsg"] = new List<string> { "The Chat Invite was changed by somebody else" };
            }

            TempData["Car"] = car;

            return RedirectToAction("ChatInvite", new { id = car.Id });
        }

        // GET: Car/Create
        [HttpGet]
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var user = context.Users.Find(userId);
            var ownedCars = context.Owners.Count(o => o.UserId == userId && o.Category == "Owner");
            if (ownedCars >= user.MaxOwned)
            {
                TempData["InfoMsgs"] = new List<string> { "Your limit of Owned Cars has been reached!" };
                return RedirectToAction("Index", "Manage");
            }
            return View();
        }

        // POST: Car/Create
        [HttpPost]
        public async Task<ActionResult> Create(CarModel car)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var user = context.Users.Find(userId);
                var ownedCars = context.Owners.Count(o => o.UserId == userId && o.Category == "Owner");
                if (ownedCars >= user.MaxOwned)
                {
                    TempData["InfoMsgs"] = new List<string> { "The limit of Owned Cars has been reached!" };
                    return RedirectToAction("Index", "Manage");
                }

                if (ModelState.IsValid)
                {
                    CarModel existingCar = context.Cars.FirstOrDefault(c => c.Plate == car.Plate && c.CountryCode == car.CountryCode);
                    if (existingCar == null)
                    {
                        context.Cars.Add(car);
                        await car.UpdateChatInviteLinkAsync();
                        context.Owners.Add(new OwnerModel(userId, car.Id));
                        var chat = new Chat(null, car.Id, DateTime.MaxValue);
                        context.Chats.Add(chat);
                        context.LastSeens.Add(new LastSeen(userId, chat.Id));
                        context.SaveChanges();
                        TempData["SuccessMsgs"] = new List<string> { "Car Added" };
                        return RedirectToAction("Index", "Manage");
                    }

                    OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == existingCar.Id);
                    string msg;
                    if (owner == null)
                    {
                        msg = "This car is already owned. If you know the owner send him a request";
                    } else if (owner.Category == "Owner")
                    {
                        msg = "You already own the car";
                    } else if (owner.Category == "CoOwner")
                    {
                        msg = "You already co-own the car";
                    } else if (owner.Category == "Invited")
                    {
                        msg = "You already are invited to co-own the car";
                    } else if (owner.Category == "Requested")
                    {
                        msg = "You already requested to co-own the car";
                    } else
                    {
                        msg = "This car is already owned.";
                    }
                    ViewBag.InfoMsgs = new List<string> { msg };
                }
                return View();
            }
            catch
            {
                return View();
            }
        }

        // GET: Car/Edit/5
        [HttpGet]
        public ActionResult Edit(string id)
        {
            string userId = User.Identity.GetUserId();
            OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);
            if ((owner == null || owner.Category != "Owner") && !User.IsInRole("Admin"))
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }
            else
            {
                ViewBag.Owned = true;
            }

            var car = context.Cars.Find(id);
            if (car == null)
            {
                TempData["SuccessMsgs"] = new List<string> { "Car Created" };
                return RedirectToAction("Index", "Manage");
            }

            return View(car);
        }

        // POST: Car/Edit/id
        [HttpPost]
        public ActionResult Edit(string id, FormCollection collection)
        {

            string userId = User.Identity.GetUserId();
            OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);
            ViewBag.Owned = !(owner == null || owner.Category != "Owner");
            if (!((bool) ViewBag.Owned || User.IsInRole("Admin")))
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            CarModel car = context.Cars.Find(id);
            string plate = car.Plate;
            string code = car.CountryCode;
            if (car == null)
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    //oldCar.Plate = car.Plate;
                    //oldCar.CountryCode = car.CountryCode;
                    //oldCar.ModelName = car.ModelName;
                    //oldCar.Color = car.Color;

                    if (TryUpdateModel(car))
                    {
                        context.SaveChanges();
                        if (car.Plate != plate || car.CountryCode != code)
                            context.Chats.Where(c => c.carId == id).Select(c => c.Id).ToList()
                                .ForEach((chat) => ChatHub.UpdateCarChat(chat, car.Plate, car.CountryCode));
                        TempData["SuccessMsgs"] = new List<string> { "Car Updated" };
                        return RedirectToAction("Details/" + id);
                    }
                    else
                    {
                        ViewBag.WarningMsgs = new List<string> { "This car plate is already registered" };
                    }
                }
            }
            catch (DbUpdateException)
            {
                ViewBag.InfoMsgs = new List<string> { "This car plate is already registered" };
            }
            catch (Exception e)
            {
                ViewBag.DangerMsgs = new List<string> { e.Message };
            }

            return View(car);
        }

        // GET: Car/RequestCoOwner
        // used send invitation to a user to become a CoOwner
        [HttpGet]
        public ActionResult RequestCoOwner()
        {
            var userId = User.Identity.GetUserId();
            var user = context.Users.Find(userId);
            var ownedCars = context.Owners.Count(o => o.UserId == userId && o.Category == "CoOwner");
            if (ownedCars >= user.MaxCoOwned)
            {
                TempData["InfoMsgs"] = new List<string> { "Your limit of CoOwned Cars has been reached!" };
                return RedirectToAction("Index", "Manage");
            }
            return View();
        }

        // Post: Car/RequestCoOwner
        [HttpPost]
        public ActionResult RequestCoOwner(FormCollection collection)
        {
            try
            {

                var userId = User.Identity.GetUserId();
                var user = context.Users.Find(userId);
                var ownedCars = context.Owners.Count(o => o.UserId == userId && o.Category == "CoOwner");
                if (ownedCars >= user.MaxCoOwned)
                {
                    TempData["InfoMsgs"] = new List<string> { "Your limit of CoOwned Cars has been reached!" };
                    return RedirectToAction("Index", "Manage");
                }

                string ownerEmail     = collection["Email"];
                string carPlate       = collection["Plate"];
                string carCountryCode = collection["CountryCode"];

                if (User.Identity.GetUserName() == ownerEmail)
                {
                    TempData["InfoMsgs"] = new List<string> { "You can't request to join a car that might belong to you" };
                    return RedirectToAction("Index", "Manage");
                }

                ApplicationUser ownerUser = context.Users.FirstOrDefault(u => u.UserName == ownerEmail);
                CarModel ownerCar = context.Cars.FirstOrDefault(c => c.Plate == carPlate && c.CountryCode == carCountryCode);
                OwnerModel owner;

                if (ownerUser == null)
                {
                    ViewBag.WarningMsgs = ((List<string>)(ViewBag.WarningMsgs ?? new List<string>()));
                    ViewBag.WarningMsgs.Add("That user doen't own a car like that");
                }

                if (ownerCar == null)
                {
                    ViewBag.WarningMsgs = ((List<string>)(ViewBag.WarningMsgs ?? new List<string>()));
                    ViewBag.WarningMsgs.Add("That user doen't own a car like that");
                }

                if (ownerUser != null && ownerCar != null)
                {
                    owner = context.Owners.FirstOrDefault(o => o.UserId == ownerUser.Id && o.CarId == ownerCar.Id && o.Category == "Owner");
                    if (owner == null)
                    {
                        ViewBag.WarningMsgs = ((List<string>)(ViewBag.WarningMsgs ?? new List<string>()));
                        ViewBag.WarningMsgs.Add("That user doen't own a car like that");
                    }
                }

                if (ViewBag.WarningMsgs != null)
                {
                    ViewBag.ownerEmail = ownerEmail;
                    ViewBag.carPlate = carPlate;
                    ViewBag.carCountryCode = carCountryCode;

                    return View();
                }

                OwnerModel coOwner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == ownerCar.Id);
                if (coOwner == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "Request Sent" };
                    context.Owners.Add(new OwnerModel(userId, ownerCar.Id, "Requested", DateTime.Now.AddDays(7)));
                    context.SaveChanges();
                }
                else if (coOwner.Category == "Owner")
                {
                    TempData["InfoMsgs"] = new List<string> { "You already own the car" };
                }
                else if (coOwner.Category == "CoOwner")
                {
                    TempData["InfoMsgs"] = new List<string> { "Already a CoOwner" };
                }
                else if (coOwner.Category == "Invited")
                {
                    TempData["InfoMsgs"] = new List<string> { "You already invited to be a CoOwner; Please Accept the invitation" };
                }
                else if (coOwner.Category == "Requested")
                {
                    TempData["InfoMsgs"] = new List<string> { "Already Requested" };
                }
                else
                {
                    // unknown tipe of Ownership => send request
                    TempData["WarningMsgs"] = new List<string> { "Unknown type of user" };
                    TempData["InfoMsgs"] = new List<string> { "Request Sent" };

                    coOwner.Category = "Requested";
                    coOwner.Expiry = DateTime.Now.AddDays(7);
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return RedirectToAction("Index", "Manage");
            }
            return RedirectToAction("Index", "Manage");
        }

        // GET: Car/InviteCoOwner/id/mail
        // used send invitation to a user to become a CoOwner
        [HttpGet]
        public ActionResult InviteCoOwner(string id)
        {
            string userId = User.Identity.GetUserId();
            OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);
            if ((owner == null || owner.Category != "Owner") && !User.IsInRole("Admin"))
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            var car = context.Cars.Find(id);
            if (car == null)
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }
            var coOwnersNumber = context.Owners.Count(o => o.CarId == id && o.Category == "CoOwner");
            if (coOwnersNumber >= car.maxCoOwners)
            {
                TempData["InfoMsgs"] = new List<string> { "The limit of CoOwners has been reached!" };
                return Redirect("../Details/" + car.Id);
            }

            return View();
        }

        // Post: Car/InviteCoOwner/id/mail
        [HttpPost]
        public ActionResult InviteCoOwner(string id, FormCollection collection)
        {
            try
            {
                string mail = collection["Email"]; // to add from collection

                string userId = User.Identity.GetUserId();
                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);
                if ((owner == null || owner.Category != "Owner") && !User.IsInRole("Admin"))
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car!" };
                    return RedirectToAction("Index", "Manage");
                }
                var car = context.Cars.Find(id);
                if (car == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car!" };
                    return RedirectToAction("Index", "Manage");
                }

                var coOwnersNumber = context.Owners.Count(o => o.CarId == id && o.Category == "CoOwner");
                if (coOwnersNumber >= car.maxCoOwners)
                {
                    TempData["InfoMsgs"] = new List<string> { "The limit of CoOwners has been reached!" };
                    return Redirect("../Details/" + car.Id);
                }

                ApplicationUser user = context.Users.FirstOrDefault(u => u.UserName == mail);
                if (user == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "User Not Found" };
                    return RedirectToAction("Details/" + id);
                }

                OwnerModel coOwner = context.Owners.FirstOrDefault(o => o.CarId == id && o.UserId == user.Id);
                if (coOwner == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "Invitation Sent" };
                    context.Owners.Add(new OwnerModel(user.Id, id, "Invited", DateTime.Now.AddDays(7)));
                    context.SaveChanges();
                }
                else if (coOwner.Category == "Owner")
                {
                    TempData["InfoMsgs"] = new List<string> { "You already own the car" };
                }
                else if (coOwner.Category == "CoOwner")
                {
                    TempData["InfoMsgs"] = new List<string> { "Already a CoOwner" };
                }
                else if (coOwner.Category == "Invited")
                {
                    TempData["InfoMsgs"] = new List<string> { "Already Invited" };
                }
                else if (coOwner.Category == "Requested")
                {
                    TempData["InfoMsgs"] = new List<string> { "That user requested to be a CoOwner; Please Accept his request" };
                }
                else
                {
                    // unknown tipe of Ownership => send invitation
                    TempData["WarningMsgs"] = new List<string> { "Unknown type of user" };
                    TempData["InfoMsgs"] = new List<string> { "Invitation Sent" };

                    coOwner.Category = "Invited";
                    coOwner.Expiry   = DateTime.Now.AddDays(7);
                    context.SaveChanges();
                }

                return RedirectToAction("Details/" + id);
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return RedirectToAction("Index", "Manage");
            }
        }

        // GET: Car/AcceptInvite/id
        [HttpGet]
        public ActionResult AcceptInvite(string id)
        {
            try
            {
                string userId = User.Identity.GetUserId();

                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);
                if (owner == null || owner.Category != "Invited")
                {
                    TempData["WarningMsgs"] = new List<string> { "The invitation might have been deleted!" };
                    return RedirectToAction("Index", "Manage");
                }

                var user = context.Users.Find(userId);
                var userCoOwnersNumber = context.Owners.Count(o => o.UserId == userId && o.Category == "CoOwner");
                if (userCoOwnersNumber >= user.MaxCoOwned)
                {
                    TempData["WarningMsgs"] = new List<string> { "You have reached your CoOwned Cars limit!" };
                    return RedirectToAction("Index", "Manage");
                }

                var car = context.Cars.Find(id);
                var carCoOwnersNumber = context.Owners.Count(o => o.CarId == id && o.Category == "CoOwner");
                if (carCoOwnersNumber >= car.maxCoOwners)
                {
                    TempData["WarningMsgs"] = new List<string> { "That Car has reached its CoOwners limit" };
                    return RedirectToAction("Index", "Manage");
                }

                owner.Category = "CoOwner";
                owner.Expiry = DateTime.MaxValue;

                // add not seen to all chats for the existing car
                context.Chats.Where(c => c.carId == car.Id).Select(c => c.Id)
                    .ToList().ForEach(chatId => context.LastSeens.Add(new LastSeen(userId, chatId)));

                context.SaveChanges();

                TempData["SuccessMsgs"] = new List<string> { "You are now a CoOwner" };
                return RedirectToAction("Details/" + id);
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return RedirectToAction("Index", "Manage");
            }
        }

        // GET: Car/OwnerAccept/id/mail
        [HttpGet]
        public ActionResult OwnerAccept(string id, string mail)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);

                if ((owner == null || owner.Category != "Owner") && !User.IsInRole("Admin"))
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                    return RedirectToAction("Index", "Manage");
                }
                var car = context.Cars.Find(id);
                if (car == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                    return RedirectToAction("Index", "Manage");
                }
                ApplicationUser userReq = context.Users.FirstOrDefault(u => u.UserName == mail);
                if (userReq == null || userId == userReq.Id)
                {
                    TempData["InfoMsgs"] = new List<string> { "That invitation might have been removed" };
                    return RedirectToAction("Details/" + id);
                }
                var carCoOwnersNumber = context.Owners.Count(o => o.CarId == id && o.Category == "CoOwner");
                if (carCoOwnersNumber >= car.maxCoOwners)
                {
                    TempData["InfoMsgs"] = new List<string> { "The Car's limit of CoOwners has been reached!" };
                    return Redirect("../Details/" + car.Id);
                }
                var userCoOwnersNumber = context.Owners.Count(o => o.UserId == userReq.Id && o.Category == "CoOwner");
                if (userCoOwnersNumber >= userReq.MaxCoOwned)
                {
                    TempData["InfoMsgs"] = new List<string> { "The User's limit of CoOwners has been reached!" };
                    return Redirect("../Details/" + car.Id);
                }

                OwnerModel coOwner = context.Owners.FirstOrDefault(o => o.UserId == userReq.Id && o.CarId == id);
                if (coOwner == null || coOwner.Category == "Owner")
                {
                    TempData["InfoMsgs"] = new List<string> { "Faulty Invitation" };
                    return RedirectToAction("Index", "Manage");
                }

                coOwner.Category = "CoOwner";
                coOwner.Expiry   = DateTime.MaxValue;

                // add not seen to all chats for the existing car
                context.Chats.Where(c => c.carId == car.Id).Select(c => c.Id)
                    .ToList().ForEach(chatId => context.LastSeens.Add(new LastSeen(userReq.Id, chatId)));

                context.SaveChanges();

                TempData["SuccessMsgs"] = new List<string> { mail + " is now a CoOwner" };
                return RedirectToAction("Details/" + id);
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return RedirectToAction("Index", "Manage");
            }
        }

        // GET: Car/RemoveCoOwner/5
        // used to remove CoOwners (or their invitation/request)
        [HttpGet]
        public ActionResult RemoveCoOwner(string carId, string mail)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == carId);

                if ((owner == null || owner.Category != "Owner") && !User.IsInRole("Admin"))
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                    return RedirectToAction("Index", "Manage");
                }

                ApplicationUser userRem = context.Users.FirstOrDefault(u => u.UserName == mail);
                if (userRem == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "User Not Found" };
                    return RedirectToAction("Details/" + carId);
                }

                OwnerModel removedOwner = context.Owners.FirstOrDefault(o => o.CarId == carId && o.UserId == userRem.Id);
                if (removedOwner == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "Not a CoOwner" };
                    return RedirectToAction("Details/" + carId);
                }
                context.Owners.Remove(removedOwner);
                var chatIds = context.Chats.Where(c => c.carId == carId).Select(c => c.Id).ToList();
                var lastSeens = context.LastSeens.Where(s => s.userId == userRem.Id && chatIds.Contains(s.chatId));
                context.LastSeens.RemoveRange(lastSeens);
                context.SaveChanges();


                string msg;

                if (removedOwner.Category == "CoOwner")
                {
                    msg = " is no longer a CoOwner";
                }
                else if (removedOwner.Category == "Invited")
                {
                    msg = " Invitation was Removed";
                }
                else if (removedOwner.Category == "Requested")
                {
                    msg = " Request was Removed";
                }
                else
                {
                    msg = " was Removed";
                }

                TempData["SuccessMsgs"] = new List<string> { mail + msg };
                return RedirectToAction("Details/" + carId);
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return RedirectToAction("Index", "Manage");
            }
        }

        // GET: Car/Delete/5
        [HttpGet]
        public ActionResult Delete(string id)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);
                ViewBag.Owned = owner?.Category == "Owner";
                if (!(owner != null || User.IsInRole("Admin")))
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                    return RedirectToAction("Index", "Manage");
                }

                if (owner?.Category == "CoOwner")
                {
                    context.Owners.RemoveRange(context.Owners.Where(o => o.CarId == id && o.UserId == userId));
                }
                else
                {
                    // context.Owners.RemoveRange(context.Owners.Where(o => o.CarId == id));

                    var car = context.Cars.Find(id);
                    if (car == null)
                    {
                        TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                    }
                    else
                    {
                        car.Delete(context);
                    }
                } 

                context.SaveChanges();

                string msg;

                if (owner?.Category == "Owner" || owner?.Category == "CoOwner")
                {
                    msg = "Car Removed";
                }
                else if (owner?.Category == "Invited")
                {
                    msg = "Invitation Removed";
                } else if (owner?.Category == "Requested")
                {
                    msg = "Request Removed";
                } else
                {
                    msg = "Removed";
                }

                TempData["SuccessMsgs"] = new List<string> { msg };
                return RedirectToAction("Index", "Manage");
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return RedirectToAction("Index", "Manage");
            }
        }

        [HttpGet]
        public ActionResult IncreaseCoOwnersLimit(string id)
        {
            string userId = User.Identity.GetUserId();
            OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);
            if ((owner == null || owner.Category != "Owner") && !User.IsInRole("Admin"))
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            var car = context.Cars.Find(id);
            if (car == null)
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return RedirectToAction("Index", "Manage");
            }

            if (car.maxCoOwners >= 100)
            {
                TempData["InfoMsgs"] = new List<string> { "This car already has the maximum number of CoOwners" };
                return Redirect("../Details/" + car.Id);
            }

            try
            {
                // no payment validation

                car.maxCoOwners += 1;
                context.SaveChanges();
                TempData["SuccessMsgs"] = new List<string> { "The limit of CoOwners has increased" };
            }
            catch
            {
                TempData["DangerMsgs"] = new List<string> { "The limit of CoOwners couldn't be increased" };
            }

            return Redirect("../Details/" + car.Id);
        }
    }
}
