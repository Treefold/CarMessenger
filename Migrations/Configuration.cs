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

            // TestUsers and their cars Seed
            ApplicationUser user;
            CarModel car;

            for (int i=1; i <= 3; ++i)
            {
                user = context.Users.First(u => u.Email == "test" + i.ToString() + "@gmail.com");
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = "test" + i.ToString() + "@gmail.com",
                        Email = "test" + i.ToString() + "@gmail.com",
                        Nickname = "Test" + i.ToString()
                    };
                    userManager.CreateAsync(user, "Ttest" + i.ToString() + ".").Wait();
                }

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
            context.SaveChanges();
        }
    }
}
