namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using CarMessenger.Models;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity;

    internal sealed class Configuration : DbMigrationsConfiguration<CarMessenger.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(CarMessenger.Models.ApplicationDbContext context)
        {
           // Roles seed
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                var roleStore = new RoleStore<IdentityRole>(context);
                roleStore.CreateAsync(new IdentityRole { Id = "0", Name = "Admin" }).Wait();
                //context.Roles.AddOrUpdate(new IdentityRole { Id = "0", Name = "Admin" });
            }

            // Administrator Seed
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            var admin = new ApplicationUser {
                UserName = "Admin@secure.db",
                Email = "Admin@secure.db",
                Nickname = "Admini"
            };
            if (!context.Users.Any(u => u.Email == admin.Email))
            {
                userManager.Create(admin, "$Uq3rP@sSvv0Rd");
                userManager.AddToRole(admin.Id, "Admin");
            }

            // Test Users
            const int N = 3;
            ApplicationUser user = null;
            for (int i = 1; i <= N; ++i)
            {
                if (!context.Users.Any(u => u.Email == "test" + i.ToString() + "@gmail.com"))
                {
                    user = new ApplicationUser
                    {
                        UserName = "test" + i.ToString() + "@gmail.com",
                        Email = "test" + i.ToString() + "@gmail.com",
                        Nickname = "Test" + i.ToString()
                    };
                    userManager.CreateAsync(user, "Ttest" + i.ToString() + ".").Wait();
                }
            }

            // Test Cars
            CarModel car = null;
            for (int i=1; i <= N; ++i)
            {
                user = context.Users.First(u => u.Email == "test" + i.ToString() + "@gmail.com");

                car = new CarModel
                {
                    Plate = "TEST00" + i.ToString(),
                    CountryCode = "TC",
                    ModelName = "TESTING",
                    Color = "None"
                };
                if (!context.Cars.Any(c => c.Plate == car.Plate && c.CountryCode == car.CountryCode))
                {
                    context.Cars.Add(car);
                    context.Owners.Add(new OwnerModel
                    {
                        CarId = car.Id,
                        UserId = user.Id,
                        Category = "Owner"
                    });
                }
            }

            // Test Chats
            Chat chat = null;
            for (int i = 1; i <= N; ++i)
            {
                car = context.Cars.First(c => c.Plate == ("TEST00" + i.ToString()) && c.CountryCode == "TC");

                chat = new Chat
                {
                    carId = car.Id
                };
                if (!context.Chats.Any(c => c.userId == chat.userId && c.carId == chat.carId))
                {
                    context.Chats.Add(chat);
                }
            }

            context.SaveChanges();
        }
    }
}
