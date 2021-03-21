namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newComputer : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.CarModels", "UniquePlateNumber");
        }
        
        public override void Down()
        {
            CreateIndex("dbo.CarModels", new[] { "Plate", "CountryCode" }, unique: true, name: "UniquePlateNumber");
        }
    }
}
