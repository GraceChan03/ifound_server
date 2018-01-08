namespace Ifound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTerritory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Territories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Image = c.String(),
                        Site = c.String(),
                        Remark = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Territories");
        }
    }
}
