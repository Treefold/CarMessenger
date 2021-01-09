namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOwnerType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OwnerModels", "Category", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OwnerModels", "Category");
        }
    }
}
