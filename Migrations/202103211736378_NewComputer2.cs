namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewComputer2 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.CarModels", new[] { "Plate", "CountryCode" }, unique: true, name: "UniquePlateNumber");
        }
        
        public override void Down()
        {
            DropIndex("dbo.CarModels", "UniquePlateNumber");
        }
    }
}
