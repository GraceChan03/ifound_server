namespace Ifound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPriceForAucWishlist : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuctionWishlists", "NewAucPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuctionWishlists", "NewAucPrice");
        }
    }
}
