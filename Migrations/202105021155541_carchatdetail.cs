namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class carchatdetail : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.CarModels", "chatInviteToken", c => c.String(nullable: false, maxLength: 64));
            AlterColumn("dbo.CarModels", "chatInviteLink", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.CarModels", "chatInviteToken", unique: true);
            CreateIndex("dbo.CarModels", "chatInviteLink", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.CarModels", new[] { "chatInviteLink" });
            DropIndex("dbo.CarModels", new[] { "chatInviteToken" });
            AlterColumn("dbo.CarModels", "chatInviteLink", c => c.String(maxLength: 128));
            AlterColumn("dbo.CarModels", "chatInviteToken", c => c.String(maxLength: 64));
        }
    }
}
