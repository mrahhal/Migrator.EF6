namespace BasicConsoleApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PostCreated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Posts", "Created", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Posts", "Created");
        }
    }
}
