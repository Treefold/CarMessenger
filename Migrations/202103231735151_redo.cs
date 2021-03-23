namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class redo : DbMigration
    {
        public override void Up()
        {           
            AddColumn("dbo.OwnerModels", "Category", c => c.String(nullable: false));
            AddColumn("dbo.OwnerModels", "Expiry", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "Nickname", c => c.String(nullable: false));
            CreateIndex("dbo.CarModels", new[] { "Plate", "CountryCode" }, unique: true, name: "UniquePlateNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.CarModels", "UniquePlateNumber");
            DropColumn("dbo.AspNetUsers", "Nickname");
            DropColumn("dbo.OwnerModels", "Expiry");
            DropColumn("dbo.OwnerModels", "Category");
            DropTable("dbo.Messages");
            DropTable("dbo.InvitationModels");
        }
    }
}
