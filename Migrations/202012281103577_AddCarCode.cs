namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCarCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CarModels", "CountryCode", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CarModels", "CountryCode");
        }
    }
}
