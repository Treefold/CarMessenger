using System;
using System.Collections.Generic;
using System.Web;
using CarMessenger.Models;
using Microsoft.AspNet.SignalR;
using System.Text.Json;

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
            Message msg = new Message(null, senderNickname, carPlate, carCountryCode, personNickname, owning, content);
            Clients.OthersInGroup(CarGroup(carPlate, carCountryCode)).addMessage(JsonSerializer.Serialize(msg));
            msg.senderEmail = senderEmail;
            context.Messages.Add(msg);
            context.SaveChanges();
        }
    }
}