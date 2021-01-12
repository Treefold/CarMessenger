namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "senderNickname", c => c.String());
            AddColumn("dbo.Messages", "personNickname", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Messages", "personNickname");
            DropColumn("dbo.Messages", "senderNickname");
        }
    }
}
