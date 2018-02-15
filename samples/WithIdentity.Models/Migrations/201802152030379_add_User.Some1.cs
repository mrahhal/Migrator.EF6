namespace WithIdentity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_UserSome1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Some1", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Some1");
        }
    }
}
