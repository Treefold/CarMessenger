namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyCarId : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.CarModels");
            DropPrimaryKey("dbo.OwnerModels");
            DropColumn("dbo.CarModels", "Id");
            AddColumn("dbo.CarModels", "Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.OwnerModels", "CarId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.CarModels", "Id");
            AddPrimaryKey("dbo.OwnerModels", new[] { "UserId", "CarId" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.OwnerModels");
            DropPrimaryKey("dbo.CarModels");
            DropColumn("dbo.CarModels", "Id");
            AddColumn("dbo.OwnerModels", "CarId", c => c.Int(nullable: false));
            AlterColumn("dbo.CarModels", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.OwnerModels", new[] { "UserId", "CarId" });
            AddPrimaryKey("dbo.CarModels", "Id");
        }
    }
}
