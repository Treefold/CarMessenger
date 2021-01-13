namespace CarMessenger.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMessagePerson : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "personMail", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Messages", "personMail");
        }
    }
}
