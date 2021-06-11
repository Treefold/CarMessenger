namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateAppDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CarModels",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 40),
                        Plate = c.String(nullable: false, maxLength: 19),
                        CountryCode = c.String(nullable: false, maxLength: 3),
                        ModelName = c.String(nullable: false, maxLength: 20),
                        Color = c.String(maxLength: 20),
                        maxCoOwners = c.Short(nullable: false),
                        chatInviteToken = c.String(nullable: false, maxLength: 64),
                        chatInviteLink = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.Plate, t.CountryCode }, unique: true, name: "UniquePlateNumber")
                .Index(t => t.chatInviteToken, unique: true)
                .Index(t => t.chatInviteLink, unique: true);
            
            CreateTable(
                "dbo.Chats",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 40),
                        userId = c.String(maxLength: 40),
                        carId = c.String(nullable: false, maxLength: 40),
                        createTime = c.DateTime(nullable: false),
                        deleteTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.carId, t.userId }, unique: true, name: "UniqueChat")
                .ForeignKey("dbo.AspNetUsers", c => c.userId, cascadeDelete: true, "ChatUserIdFK")
                .ForeignKey("dbo.CarModels", c => c.carId, cascadeDelete: true, "ChatCarIdFK");
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 40),
                        chatId = c.String(nullable: false, maxLength: 40),
                        userId = c.String(nullable: false, maxLength: 40),
                        content = c.String(nullable: false, maxLength: 500),
                        sendTime = c.DateTime(nullable: false),
                        expiry = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", m => m.userId, cascadeDelete: false, "MessUserIdFK")
                .ForeignKey("dbo.Chats", m => m.chatId, cascadeDelete: true, "MessChatIdFK");
            
            CreateTable(
                "dbo.OwnerModels",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 40),
                        CarId = c.String(nullable: false, maxLength: 40),
                        Category = c.String(nullable: false, maxLength: 10),
                        Expiry = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.CarId })
                .ForeignKey("dbo.AspNetUsers", o => o.UserId, cascadeDelete: true, "OwnerUserIdFK")
                .ForeignKey("dbo.CarModels", o => o.CarId, cascadeDelete: true, "OwnerCarIdFK")
                .Index(t => t.CarId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 40),
                        Name = c.String(nullable: false, maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 40),
                        RoleId = c.String(nullable: false, maxLength: 40),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 40),
                        Nickname = c.String(nullable: false, maxLength: 30),
                        MaxOwned = c.Short(nullable: false),
                        MaxCoOwned = c.Short(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 40),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 40),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.OwnerModels", "CarId", "dbo.CarModels");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.OwnerModels", new[] { "CarId" });
            DropIndex("dbo.Chats", "UniqueChat");
            DropIndex("dbo.CarModels", new[] { "chatInviteLink" });
            DropIndex("dbo.CarModels", new[] { "chatInviteToken" });
            DropIndex("dbo.CarModels", "UniquePlateNumber");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.OwnerModels");
            DropTable("dbo.Messages");
            DropTable("dbo.Chats");
            DropTable("dbo.CarModels");
        }
    }
}
