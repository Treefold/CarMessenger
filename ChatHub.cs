using System;
using System.Collections.Generic;
using System.Web;
using CarMessenger.Models;
using Microsoft.AspNet.SignalR;

namespace CarMessenger
{
    public class ChatHub : Hub
    {
        private ApplicationDbContext context;

        public ChatHub()
        {
            context = new ApplicationDbContext();
        }
        public void Send (string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(name, message);
        }
        private string CarGroup (string carPlate, string carCountryCode)
        {
            return carPlate + "_" + carCountryCode;
        }
        public void JoinCar (string carPlate, string carCountryCode)
        {
            Groups.Add(Context.ConnectionId, CarGroup(carPlate, carCountryCode));
        }
        public void JoinCars (List<(string plate, string countryCode)> cars)
        {
            foreach (var car in cars)
            {
                JoinCar(car.plate, car.countryCode);
            }
        }
        public void MessageCar(string senderEmail, string senderNickname, string carPlate, string carCountryCode, string personNickname, bool owning, string content)
        {
            Clients.Group(CarGroup(carPlate, carCountryCode)).addMessage(senderNickname, content);
            Message msg = new Message(senderEmail, senderNickname, carPlate, carCountryCode, personNickname, owning, content);
            context.Messages.Add(msg);
            context.SaveChanges();
        }
    }
}