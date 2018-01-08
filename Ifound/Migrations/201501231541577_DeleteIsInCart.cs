namespace Ifound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteIsInCart : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AuctionWishlists", "IsInCart");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AuctionWishlists", "IsInCart", c => c.Boolean(nullable: false));
        }
    }
}
