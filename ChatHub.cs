using System;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace CarMessenger
{
    public class ChatHub : Hub
    {
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
        public void MessageCar(string mail, string nickname, string carPlate, string carCountryCode, string message)
        {
            Clients.Group(CarGroup(carPlate, carCountryCode)).addMessage(nickname, message);
        }
    }
}