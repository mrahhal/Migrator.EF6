namespace BasicConsoleApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_Some : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Blogs", "Some", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Blogs", "Some");
        }
    }
}
