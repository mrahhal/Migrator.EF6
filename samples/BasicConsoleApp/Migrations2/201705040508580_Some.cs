namespace BasicConsoleApp.Migrations2
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Some : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Foos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Foos");
        }
    }
}
