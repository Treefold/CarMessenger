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
    public class CarHub : Hub
    {
        private static ApplicationDbContext contextdb = ApplicationDbContext.GetApplicationDbContext(); 
        private static string userPrefix = "User_";
        private static string carGroupPrefix = "Car_";
        private static IHubContext carHub = GlobalHost.ConnectionManager.GetHubContext<CarHub>();

        public CarHub() {}

        public void JoinMyNotification()
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

                Groups.Add(Context.ConnectionId, userPrefix + userId);
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

        public void JoinNewCar(string carId)
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

                if (CarModel.IsOwnedBy(contextdb, userId, carId))
                {
                    // all validations passed => it's safe to add it
                    JoinMyCarTrusted(carId);
                }
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

        public static void RemoveStatus (string userId, string carId)
        {
            // only the server can call this function
            // not verified, already trusted
            try
            {
                if (carHub != null)
                {
                    // notify the target user
                    carHub.Clients.Group(userPrefix + userId).RemoveStatus(carId);
                    // TODO: Notify the car
                }
            }
            catch
            {
                // do nothing
            }
        }

        public static void NotifyNewOwnedCar(string userId, CarModel car)
        {
            // only the server can call this function
            // not verified, already trusted
            try
            {
                if (carHub != null)
                {
                    // notify the target user
                    carHub.Clients.Group(userPrefix + userId).AddOwnedCar(car);
                    // TODO: Notify the car
                }
            }
            catch
            {
                // do nothing
            }
        }

        public static void NotifyNewCoOwnedCar(string userId, CarModel car)
        {
            // only the server can call this function
            // not verified, already trusted
            try
            {
                if (carHub != null)
                {
                    // notify the target user
                    carHub.Clients.Group(userPrefix + userId).AddCoOwnedCar(car);
                    // TODO: Notify the car
                }
            }
            catch
            {
                // do nothing
            }
        }

        public static void NotifyNewInvitation(string userId, CarModel car)
        {
            // only the server can call this function
            // not verified, already trusted
            try
            {
                if (carHub != null)
                {
                    // notify the target user
                    carHub.Clients.Group(userPrefix + userId).AddInvitation(car);
                    // TODO: Notify the car
                }
            }
            catch
            {
                // do nothing
            }
        }

        public static void NotifyNewRequest(string userId, CarModel car)
        {
            // only the server can call this function
            // not verified, already trusted
            try
            {
                if (carHub != null)
                {
                    // notify the target user
                    carHub.Clients.Group(userPrefix + userId).AddRequest(car);
                    // TODO: Notify the car
                }
            }
            catch
            {
                // do nothing
            }
        }
    }
}