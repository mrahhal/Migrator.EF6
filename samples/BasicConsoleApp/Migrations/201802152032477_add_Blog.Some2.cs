namespace BasicConsoleApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_BlogSome2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Blogs", "Some2", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Blogs", "Some2");
        }
    }
}
