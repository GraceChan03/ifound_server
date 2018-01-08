namespace Ifound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPraiseAgain : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Praises",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Praises");
        }
    }
}
