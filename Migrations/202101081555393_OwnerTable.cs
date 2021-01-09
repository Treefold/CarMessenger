namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OwnerTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OwnerModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false),
                        CarId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.AspNetUsers", "PlateNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "PlateNumber", c => c.String());
            DropTable("dbo.OwnerModels");
        }
    }
}
