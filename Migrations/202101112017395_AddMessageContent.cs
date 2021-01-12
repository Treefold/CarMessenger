namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMessageContent : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "content", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Messages", "content");
        }
    }
}
