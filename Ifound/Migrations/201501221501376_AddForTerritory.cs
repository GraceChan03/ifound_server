namespace Ifound.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddForTerritory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Territories", "Time", c => c.String());
            AddColumn("dbo.Territories", "Praise", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Territories", "Praise");
            DropColumn("dbo.Territories", "Time");
        }
    }
}
