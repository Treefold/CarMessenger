namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNickname : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Nickname", c => c.String(nullable: false));
        }
        
        public override void Down()
        {            
            DropColumn("dbo.AspNetUsers", "Nickname");
        }
    }
}
