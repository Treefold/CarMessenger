namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOwnerExpiry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OwnerModels", "Expiry", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OwnerModels", "Expiry");
        }
    }
}
