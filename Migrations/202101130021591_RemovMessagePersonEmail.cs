namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovMessagePersonEmail : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Messages", "personMail");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Messages", "personMail");
        }
    }
}
