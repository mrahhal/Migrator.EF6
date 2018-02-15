namespace BasicConsoleApp.Migrations2
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_FooSome : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Foos", "Some", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Foos", "Some");
        }
    }
}
