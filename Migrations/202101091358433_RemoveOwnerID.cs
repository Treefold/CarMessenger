namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveOwnerID : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.OwnerModels");
            AlterColumn("dbo.OwnerModels", "UserId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.OwnerModels", new[] { "UserId", "CarId" });
            DropColumn("dbo.OwnerModels", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OwnerModels", "Id", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.OwnerModels");
            AlterColumn("dbo.OwnerModels", "UserId", c => c.String(nullable: false));
            AddPrimaryKey("dbo.OwnerModels", "Id");
        }
    }
}
