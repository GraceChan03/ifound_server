namespace Ifound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPraise : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Territories", "PraiseTimes", c => c.Int(nullable: false));
            DropColumn("dbo.Territories", "Praise");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Territories", "Praise", c => c.Int(nullable: false));
            DropColumn("dbo.Territories", "PraiseTimes");
        }
    }
}
