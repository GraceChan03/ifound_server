namespace Ifound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteFromAuctions : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Auctions", "AucPrice");
            DropColumn("dbo.Auctions", "LauchTime");
            DropColumn("dbo.Auctions", "Duration");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Auctions", "Duration", c => c.Int(nullable: false));
            AddColumn("dbo.Auctions", "LauchTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.Auctions", "AucPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
