namespace EventManagementApp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAppLogTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppLog",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Level = c.String(nullable: false, maxLength: 50),
                    Message = c.String(nullable: false),
                    Exception = c.String(),
                    TimeStamp = c.DateTime(nullable: false),
                    Properties = c.String(),
                })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.AppLog");
        }
    }
}
