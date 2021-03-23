namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateChatMessages : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                  "dbo.Messages",
                  c => new
                  {
                      Id = c.String(nullable: false, maxLength: 128),
                      senderEmail = c.String(),
                      senderNickname = c.String(),
                      carPlate = c.String(nullable: false),
                      carCountryCode = c.String(nullable: false),
                      personNickname = c.String(),
                      owning = c.Boolean(nullable: false),
                      content = c.String(nullable: false),
                      sendTime = c.DateTime(nullable: false),
                      expiry = c.DateTime(nullable: false),
                  })
                  .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
        }
    }
}
