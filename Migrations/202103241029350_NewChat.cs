namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewChat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                 "dbo.Chats",
                 c => new
                 {
                     Id = c.String(nullable: false, maxLength: 128),
                     userId = c.String(nullable: false, maxLength: 128),
                     carId = c.String(nullable: false, maxLength: 128),
                 })
                 .PrimaryKey(t => t.Id);
        }
        
        public override void Down()
        {
        }
    }
}
