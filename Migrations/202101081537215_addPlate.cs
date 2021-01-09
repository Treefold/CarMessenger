namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPlate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PlateNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "PlateNumber");
        }
    }
}
