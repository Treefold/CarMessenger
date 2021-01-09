using CarMessenger.Models;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity.Infrastructure;

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
            var car = context.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
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
            var car = context.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        // POST: Car/Edit/id
        [HttpPost]
        public ActionResult Edit(string id, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    CarModel car = context.Cars.Find(id);
                    if (car == null)
                    {
                        return HttpNotFound();
                    }

                    //oldCar.Plate = car.Plate;
                    //oldCar.CountryCode = car.CountryCode;
                    //oldCar.ModelName = car.ModelName;
                    //oldCar.Color = car.Color;

                    if (TryUpdateModel(car))
                    {
                        context.SaveChanges();
                        return RedirectToAction("Details/" + id);
                    }
                    else
                    {
                        ViewBag.errMsg = "This car plate is already registered";
                    }
                }
            }
            catch (DbUpdateException)
            {
                ViewBag.errMsg = "This car plate is already registered";
                return View(id);
            }
            catch (Exception e)
            {
                return Json(new { error_message = e.Message }, JsonRequestBehavior.AllowGet);
            }

            return View(id);
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
                    return HttpNotFound();
                }

                context.Owners.Remove(owner);

                CarModel car = context.Cars.Find(id);
                if (car != null)
                {
                    context.Cars.Remove(car);
                }

                context.SaveChanges();

                return RedirectToAction("../Manage");
            }
            catch (Exception e)
            {
                return Json(new { error_message = e.Message }, JsonRequestBehavior.AllowGet);
            }
            //return View(id);
        }
    }
}
