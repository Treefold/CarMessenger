using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CarMessenger.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here

            userIdentity.AddClaim(new Claim("Nickname", this.Nickname));
            //userIdentity.AddClaim(new Claim("MaxOwned", this.MaxOwned.ToString()));
            //userIdentity.AddClaim(new Claim("MaxCoOwned", this.MaxCoOwned.ToString()));

            return userIdentity;
        }

        [Required]
        [RegularExpression(@"[A-Za-z0-9]{2,30}", ErrorMessage = ("Your nickname should have only letters and numbers (min 2, max30)"))]
        [StringLength(30, ErrorMessage = "User Nickname excedeed length limit")]
        public string Nickname { get; set; }

        [Required]
        public Int16 MaxOwned { get; set; } = 2;

        [Required]
        public Int16 MaxCoOwned { get; set; } = 3;
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private static ApplicationDbContext AppBdContextEntity = new ApplicationDbContext();

        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<CarModel> Cars { get; set; }
        public DbSet<OwnerModel> Owners { get; set; }

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext GetApplicationDbContext()
        {
            return AppBdContextEntity;
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}