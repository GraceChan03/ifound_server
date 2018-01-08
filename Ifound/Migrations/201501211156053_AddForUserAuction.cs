namespace Ifound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddForUserAuction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAuctions", "ProductScore", c => c.Single());
            AddColumn("dbo.UserAuctions", "ServiceScore", c => c.Single());
            AddColumn("dbo.UserAuctions", "OrderProcessingScore", c => c.Single());
            AddColumn("dbo.UserAuctions", "TotalPoints", c => c.Single());
            AddColumn("dbo.UserAuctions", "CompleteDealTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserAuctions", "CompleteDealTime");
            DropColumn("dbo.UserAuctions", "TotalPoints");
            DropColumn("dbo.UserAuctions", "OrderProcessingScore");
            DropColumn("dbo.UserAuctions", "ServiceScore");
            DropColumn("dbo.UserAuctions", "ProductScore");
        }
    }
}
