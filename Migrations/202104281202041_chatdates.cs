namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class chatdates : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Chats", "createTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.Chats", "deleteTime", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Chats", "deleteTime");
            DropColumn("dbo.Chats", "createTime");
        }
    }
}
