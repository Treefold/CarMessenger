using CarMessenger.Models;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity.Infrastructure;
using System.Collections.Generic;

namespace CarMessenger.Controllers
{
    [Authorize]
    public class CarsController : Controller
    {
        private ApplicationDbContext context;

        public CarsController()
        {
            context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            context.Dispose();
            base.Dispose(disposing);
        }

        // GET: Car
        [HttpGet]
        [Authorize(Roles = "SuperUser")]
        public ActionResult Index()
        {
            ViewData["cars"] = context.Cars.ToList();

            return View();
        }

        // GET: Car/Details/
        [HttpGet]
        public ActionResult Details(string id)
        {
            string userId = User.Identity.GetUserId();
            OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);

            if (owner == null)
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return Redirect("../../Manage");
            }

            var car = context.Cars.Find(id);
            if (car == null)
            {
                TempData["InfoMsgs"] = new List<string> { "We coudn't find that car" };
                return Redirect("../../Manage");
            }
            
            if (owner.Category == "Owner")
            {
                ViewBag.Owned = true;
                ViewBag.OwnerName = User.Identity.GetUserName();
            }
            else if (owner.Category == "CoOwner")
            {
                OwnerModel realOwner = context.Owners.FirstOrDefault(o => o.CarId == id && o.Category == "Owner");
                if (realOwner == null)
                {
                    owner.Category = "Owner";
                    context.SaveChanges();
                    ViewBag.Owned = true;
                    ViewBag.OwnerName = User.Identity.GetUserName();
                    if (TempData["InfoMsgs"] == null)
                    {
                        TempData["InfoMsgs"] = new List<string> { "This car didn't have an Owner. Now it choosed you!" };
                    }
                    else
                    {
                        ViewBag.Owned = false;
                        //List<string> infoMsgs = TempData["InfoMsgs"] as List<string>;
                        //infoMsgs.Add("This car didn't have an Owner. Now it choosed you!");
                        //TempData["InfoMsgs"] = infoMsgs;
                        (TempData["InfoMsgs"] as List<string>).Add("This car didn't have an Owner. Now it choosed you!");
                    }
                }
            }
            else
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return Redirect("../../Manage");
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

            if (owner.Category == "Owner")
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

        // GET: Car/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Car/Create
        [HttpPost]
        public ActionResult Create(CarModel car)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    context.Cars.Add(car);
                    context.Owners.Add(new OwnerModel(User.Identity.GetUserId(), car.Id));
                    context.SaveChanges();
                    TempData["SuccessMsgs"] = new List<string> { "Car Added" };
                    return RedirectToAction("../Manage");
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
            if (owner == null || owner.Category != "Owner")
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return Redirect("../../Manage");
            }
            else
            {
                ViewBag.Owned = true;
            }

            var car = context.Cars.Find(id);
            if (car == null)
            {
                TempData["SuccessMsgs"] = new List<string> { "Car Created" };
                return Redirect("../../Manage");
            }

            return View(car);
        }

        // POST: Car/Edit/id
        [HttpPost]
        public ActionResult Edit(string id, FormCollection collection)
        {

            string userId = User.Identity.GetUserId();
            OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);
            if (owner == null || owner.Category != "Owner")
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return Redirect("../../Manage");
            }
            else
            {
                ViewBag.Owned = true;
            }

            var car = context.Cars.Find(id);
            if (car == null)
            {
                TempData["InfoMsgs"] = new List<string> { "We coudn't find that car" };
                return Redirect("../../Manage");
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
                        TempData["SuccessMsgs"] = new List<string> { "Car Updated" };
                        return RedirectToAction("Details/" + id);
                    }
                    else
                    {
                        TempData["WarningMsgs"] = new List<string> { "This car plate is already registered" };
                    }
                }
            }
            catch (DbUpdateException)
            {
                TempData["InfoMsgs"] = new List<string> { "This car plate is already registered" };
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
            }

            return View(id);
        }

        // GET: Car/RequestCoOwner
        // used send invitation to a user to become a CoOwner
        [HttpGet]
        public ActionResult RequestCoOwner()
        {
            return View();
        }

        // Post: Car/RequestCoOwner
        [HttpPost]
        public ActionResult RequestCoOwner(FormCollection collection)
        {
            try
            {
                string ownerEmail     = collection["Email"];
                string carPlate       = collection["Plate"];
                string carCountryCode = collection["CountryCode"];
                string userID         = User.Identity.GetUserId();

                if (User.Identity.GetUserName() == ownerEmail)
                {
                    TempData["InfoMsgs"] = new List<string> { "You can't request to join a car that might belong to you" };
                    return Redirect("../Manage");
                }

                ApplicationUser ownerUser = context.Users.FirstOrDefault(u => u.UserName == ownerEmail);
                CarModel ownerCar = context.Cars.FirstOrDefault(c => c.Plate == carPlate && c.CountryCode == carCountryCode);
                OwnerModel owner;

                if (ownerUser == null)
                {
                    ViewBag.WarningMsgs = ((List<string>)(ViewBag.WarningMsgs ?? new List<string>()));
                    ViewBag.WarningMsgs.Add("User Not Found");
                }

                if (ownerCar == null)
                {
                    ViewBag.WarningMsgs = ((List<string>)(ViewBag.WarningMsgs ?? new List<string>()));
                    ViewBag.WarningMsgs.Add("Car Not Found");
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

                OwnerModel coOwner = context.Owners.FirstOrDefault(o => o.UserId == userID && o.CarId == ownerCar.Id);
                if (coOwner == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "Request Sent" };
                    context.Owners.Add(new OwnerModel(userID, ownerCar.Id, "Requested", DateTime.Now.AddDays(7)));
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
                return Redirect("../Manage");
            }
            return Redirect("../Manage");
        }

        // GET: Car/InviteCoOwner/id/mail
        // used send invitation to a user to become a CoOwner
        [HttpGet]
        public ActionResult InviteCoOwner(string id)
        {
            string userId = User.Identity.GetUserId();
            OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);
            if (owner == null || owner.Category != "Owner")
            {
                TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                return Redirect("../../Manage");
            }

            var car = context.Cars.Find(id);
            if (car == null)
            {
                TempData["InfoMsgs"] = new List<string> { "We coudn't find that car" };
                return Redirect("../../Manage");
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
                if (owner == null || owner.Category != "Owner")
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car (You can't invite people)" };
                    return Redirect("../../Manage");
                }

                ApplicationUser user = context.Users.FirstOrDefault(u => u.UserName == mail);
                if (user == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "User Not Found" };
                    return RedirectToAction("Details/" + id);
                }

                if (context.Cars.Find(id) == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "We coudn't find that car" };
                    return Redirect("../../Manage");
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
                return Redirect("../../Manage");
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
                    TempData["InfoMsgs"] = new List<string> { "You invitation is not valid" };
                    return Redirect("../../Manage");
                }

                owner.Category = "CoOwner";
                owner.Expiry = DateTime.MaxValue;
                context.SaveChanges();

                TempData["SuccessMsgs"] = new List<string> { "You are now a CoOwner" };
                return RedirectToAction("Details/" + id);
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return Redirect("../../Manage");
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

                if (owner == null || owner.Category != "Owner")
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car (You can't invite people)" };
                    return Redirect("../../Manage");
                }

                ApplicationUser user = context.Users.FirstOrDefault(u => u.UserName == mail);
                if (user == null || userId == user.Id)
                {
                    TempData["InfoMsgs"] = new List<string> { "User Not Found" };
                    return RedirectToAction("Details/" + id);
                }

                OwnerModel coOwner = context.Owners.FirstOrDefault(o => o.UserId == user.Id && o.CarId == id);
                if (coOwner == null || coOwner.Category == "Owner")
                {
                    TempData["InfoMsgs"] = new List<string> { "We coudn't find that car" };
                    return Redirect("../../Manage");
                }

                coOwner.Category = "CoOwner";
                coOwner.Expiry   = DateTime.MaxValue;
                context.SaveChanges();

                TempData["SuccessMsgs"] = new List<string> { mail + " is now a CoOwner" };
                return RedirectToAction("Details/" + id);
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return Redirect("../../Manage");
            }
        }

        // GET: Car/RemoveCoOwner/5
        // used to remove CoOwners (or their invitation/request)
        [HttpGet]
        public ActionResult RemoveCoOwner(string id, string mail)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                OwnerModel owner = context.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == id);

                if (owner == null || owner.Category != "Owner")
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                    return Redirect("../../Manage");
                }

                ApplicationUser user = context.Users.FirstOrDefault(u => u.UserName == mail);
                if (user == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "User Not Found" };
                    return RedirectToAction("Details/" + id);
                }

                OwnerModel removedOwner = context.Owners.FirstOrDefault(o => o.CarId == id && o.UserId == user.Id);
                if (removedOwner == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "Not a CoOwner" };
                    return RedirectToAction("Details/" + id);
                }
                context.Owners.Remove(removedOwner);
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
                return RedirectToAction("Details/" + id);
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return Redirect("../../Manage");
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
                if (owner == null)
                {
                    TempData["InfoMsgs"] = new List<string> { "That was not your car" };
                    return Redirect("../../Manage");
                }
                else
                {
                    ViewBag.Owned = true;
                }

                if (owner.Category == "Owner")
                {
                    context.Owners.RemoveRange(context.Owners.Where(o => o.CarId == id));

                    var car = context.Cars.Find(id);
                    if (car == null)
                    {
                        TempData["InfoMsgs"] = new List<string> { "We coudn't find that car" };
                    }
                    else
                    {
                        context.Cars.Remove(car);
                    }
                } 
                else
                {
                    context.Owners.RemoveRange(context.Owners.Where(o => o.CarId == id && o.UserId == userId));
                }

                context.SaveChanges();

                string msg;

                if (owner.Category == "Owner" || owner.Category == "CoOwner")
                {
                    msg = "Car Removed";
                }
                else if (owner.Category == "Invited")
                {
                    msg = "Invitation Removed";
                } else if (owner.Category == "Requested")
                {
                    msg = "Request Removed";
                } else
                {
                    msg = "Removed";
                }

                TempData["SuccessMsgs"] = new List<string> { msg };
                return RedirectToAction("../Manage");
            }
            catch (Exception e)
            {
                TempData["DangerMsgs"] = new List<string> { e.Message };
                return Redirect("../../Manage");
            }
        }
    }
}
