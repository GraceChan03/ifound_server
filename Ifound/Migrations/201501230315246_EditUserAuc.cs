namespace Ifound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EditUserAuc : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserAuctions", "Evaluated", c => c.Boolean(nullable: false));
            AlterColumn("dbo.UserAuctions", "CompleteDealTime", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserAuctions", "CompleteDealTime", c => c.DateTime());
            DropColumn("dbo.UserAuctions", "Evaluated");
        }
    }
}
