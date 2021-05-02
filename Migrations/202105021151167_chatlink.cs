namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class chatlink : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CarModels", "chatInviteLink", c => c.String(maxLength: 128));
            AlterColumn("dbo.CarModels", "chatInviteToken", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CarModels", "chatInviteToken", c => c.String(maxLength: 128));
            DropColumn("dbo.CarModels", "chatInviteLink");
        }
    }
}
