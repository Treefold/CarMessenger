namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMessagOwning : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "owning", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Messages", "owning");
        }
    }
}
