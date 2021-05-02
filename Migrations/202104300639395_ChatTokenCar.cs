namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChatTokenCar : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CarModels", "chatInviteToken", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.CarModels", "chatInviteToken", c => c.String());
        }
    }
}
