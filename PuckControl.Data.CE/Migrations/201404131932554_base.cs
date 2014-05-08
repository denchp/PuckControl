namespace PuckControl.Data.CE.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _base : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Scores",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FinalScore = c.Int(nullable: false),
                        Game = c.String(maxLength: 4000),
                        Rank = c.Int(),
                        Monitor = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Modified = c.DateTime(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 4000),
                        BirthYear = c.Int(nullable: false),
                        Avatar = c.Binary(maxLength: 4000),
                        Created = c.DateTime(nullable: false),
                        Modified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SettingOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 4000),
                        Value = c.String(maxLength: 4000),
                        IsSelected = c.Boolean(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Modified = c.DateTime(nullable: false),
                        Setting_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Settings", t => t.Setting_Id)
                .Index(t => t.Setting_Id);
            
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Module = c.String(maxLength: 4000),
                        Section = c.String(maxLength: 4000),
                        Key = c.String(maxLength: 4000),
                        Note = c.String(maxLength: 4000),
                        Created = c.DateTime(nullable: false),
                        Modified = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SettingOptions", "Setting_Id", "dbo.Settings");
            DropForeignKey("dbo.Scores", "User_Id", "dbo.Users");
            DropIndex("dbo.SettingOptions", new[] { "Setting_Id" });
            DropIndex("dbo.Scores", new[] { "User_Id" });
            DropTable("dbo.Settings");
            DropTable("dbo.SettingOptions");
            DropTable("dbo.Users");
            DropTable("dbo.Scores");
        }
    }
}
