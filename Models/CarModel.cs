﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using RestSharp;
using Newtonsoft.Json;

namespace CarMessenger.Models
{
    public class InvitationModel
    {
        [Key]
        [Required]
        [Display(Name = "User Email")]
        [EmailAddress]
        public string Email { get; set; }

        public InvitationModel()
        {
        }

        public InvitationModel(string email)
        {
            Email = email;
        }
    }
   
    public class RequestModel
    {
        [Key]
        [Required]
        [Display(Name = "Owner Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        [RegularExpression(@"[0-9A-Z][0-9A-Z-]{3,8}[0-9A-Z]", ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        [Display(Name = "Car Plate Number")]
        public string Plate { get; set; }

        [Required(ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))] // from the plate
        [RegularExpression(@"[A-Z]{1,3}", ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))]
        [Display(Name = "Car Country Code")]
        public string CountryCode { get; set; }

        public RequestModel()
        {
        }

        public RequestModel(string email, string plate, string countryCode)
        {
            Email = email;
            Plate = plate;
            CountryCode = countryCode;
        }
    }

    public class CarModel
    {
        private static readonly RestClient client;
        private static readonly string HOST = "https://192.168.42.249:45455";

        [Key]
        [StringLength(40, ErrorMessage = "GUID excedeed length limit")]
        public string Id { get; private set; } = Guid.NewGuid().ToString();

        [Required(ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        [RegularExpression(@"[0-9A-Z][0-9A-Z-]{3,8}[0-9A-Z]", ErrorMessage = ("Please enter the Plate Number in UPPER CASE without spaces (Ex: B123ABC)"))]
        [Index("UniquePlateNumber", 1, IsUnique = true)]
        [StringLength(19, ErrorMessage = "CarPlate excedeed length limit")]
        [Display(Name = "Car Plate Number")]
        public string Plate { get; set; }

        [Required(ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))] // from the plate
        [RegularExpression(@"[A-Z]{1,3}", ErrorMessage = ("Please enter the Country Code in UPPER CASE (Ex: RO)"))]
        [Index("UniquePlateNumber", 2, IsUnique = true)]
        [StringLength(3, ErrorMessage = "Car CountryCode excedeed length limit")]
        [Display(Name = "Car Country Code")]
        public string CountryCode { get; set; }

        [Required]
        [RegularExpression(@"[A-Za-z0-9 ]{1,20}", ErrorMessage = ("Please enter the car model (only numbers and letters)"))]
        [StringLength(20, ErrorMessage = "Car ModelName excedeed length limit")]
        [Display(Name = "Car Model")]
        public string ModelName { get; set; }

        [Required]
        [RegularExpression(@"[A-Za-z0-9 ]{1,20}", ErrorMessage = ("Please enter the color (only numbers and letters)"))]
        [StringLength(20, ErrorMessage = "Car Color excedeed length limit")]
        [Display(Name = "Car Color")]
        public string Color { get; set; }

        [Required]
        public Int16 maxCoOwners { get; set; } = 4;

        [Required]
        [StringLength(64)]
        [Index(IsUnique = true)]
        public string chatInviteToken { get; private set; } = Guid.NewGuid().ToString().ToUpper();

        [Required]
        [StringLength(128)]
        [Index(IsUnique = true)]
        public string chatInviteLink { get; private set; }

        static CarModel()
        {
            client = new RestClient("https://api-ssl.bitly.com/v4/shorten") {Timeout = -1};
        }

        public CarModel(string plate, string countryCode, string modelName, string color, bool shorten = false)
        {
            this.Plate       = plate       ?? throw new ArgumentNullException(nameof(plate));
            this.CountryCode = countryCode ?? throw new ArgumentNullException(nameof(countryCode));
            this.ModelName   = modelName   ?? throw new ArgumentNullException(nameof(modelName));
            this.Color       = color;
            this.UpdateChatInviteLinkSync();
        }

        public CarModel()
        { 
            this.Plate          = "Unknown";
            this.CountryCode    = "Unknown";
            this.ModelName      = "Unknown";
            this.Color          = "Unknown";
            this.UpdateChatInviteLinkSync();
        }
        public bool IsOwnedBy(ApplicationDbContext contextdb, string userId)
        {
            OwnerModel owner = contextdb.Owners.FirstOrDefault(o => o.UserId == userId && o.CarId == this.Id);
            if (owner == null)
            {
                return false; // invalid attempt - inexistent relationship between the user and the car
            }
            if (owner.HasExpired())
            {
                owner.Delete(contextdb);
                return false; // invalid attempt - this is no longer available
            }
            if (!owner.Owns())
            {
                return false; // access denied - doesn't own the car
            }
            // else: owns the car => OK
            return true; // OK
        }

        public static bool IsOwnedBy(ApplicationDbContext contextdb, string userId, string carId)
        {
            // chat validation
            CarModel car = contextdb.Cars.Find(carId); // might fail, but catched (it's alright)
            if (car == null)
            {
                return false; // invalid attempt - inexistent chat
            }

            return car.IsOwnedBy(contextdb, userId);
        }

        public async Task<bool> GenerateNewChatInviteToken(bool shorten = false)
        {
            this.chatInviteToken = Guid.NewGuid().ToString().ToUpper();
            return await UpdateChatInviteLinkAsync(shorten);
        }

        public bool UpdateChatInviteLinkSync()
        {
            if (this.chatInviteToken == null) return false;

            this.chatInviteLink = HOST + "/Home/NewChatInvite/?token=" + chatInviteToken;

            return true;
        }

        public async Task<bool> UpdateChatInviteLinkAsync(bool shorten = false)
        {
            if (this.chatInviteToken == null) return false;
            this.chatInviteLink = null;

            if (shorten)
            {
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "4a9a89cf2fb309371f4e8a4604b1763e18d18f87");
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter(
                    "application/json", 
                    "{\r\n  \"long_url\": \""+ HOST +"/Home/NewChatInvite/?token="+ this.chatInviteToken + "\"\r\n}",
                    ParameterType.RequestBody
                );
                IRestResponse response = await client.ExecuteAsync(request);

                if (response != null && response.IsSuccessful)
                {
                    dynamic json = JsonConvert.DeserializeObject(response.Content);
                    this.chatInviteLink = json["link"];
                }
            }

            if (this.chatInviteLink == null)
                this.chatInviteLink = HOST + "/Home/NewChatInvite/?token=" + chatInviteToken;

            return true;
        }
        public void Delete(ApplicationDbContext context)
        {
            // notify car owners
            // TODO

            //notify all members in a chat with the car
            context.Chats.Where(c => c.carId == this.Id).ToList()
                .ForEach(chat => chat.Delete(context));

            context.Owners.Where(o => o.CarId == this.Id).ToList()
                .ForEach(o => o.Delete(context));

            context.Cars.Remove(this); // remove this chat
        }
    }
}