namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewMessage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "chatId", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Messages", "userId", c => c.String(nullable: false, maxLength: 128));
            DropColumn("dbo.Messages", "senderEmail");
            DropColumn("dbo.Messages", "senderNickname");
            DropColumn("dbo.Messages", "carPlate");
            DropColumn("dbo.Messages", "carCountryCode");
            DropColumn("dbo.Messages", "personNickname");
            DropColumn("dbo.Messages", "owning");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Messages", "owning", c => c.Boolean(nullable: false));
            AddColumn("dbo.Messages", "personNickname", c => c.String());
            AddColumn("dbo.Messages", "carCountryCode", c => c.String(nullable: false));
            AddColumn("dbo.Messages", "carPlate", c => c.String(nullable: false));
            AddColumn("dbo.Messages", "senderNickname", c => c.String());
            AddColumn("dbo.Messages", "senderEmail", c => c.String());
            DropColumn("dbo.Messages", "userId");
            DropColumn("dbo.Messages", "chatId");
        }
    }
}
