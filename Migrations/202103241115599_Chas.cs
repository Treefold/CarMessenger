namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Chas : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Chats", "UniqueChat");
            DropTable("dbo.Chats");
            CreateTable(
                "dbo.Chats",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        userId = c.String(maxLength: 128),
                        carId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.carId, t.userId }, unique: true, name: "UniqueChat");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Chats", "UniqueChat");
            DropTable("dbo.Chats");
        }
    }
}
