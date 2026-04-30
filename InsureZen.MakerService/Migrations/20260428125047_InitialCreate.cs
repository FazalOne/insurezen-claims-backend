using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsureZen.MakerService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InsuranceCompany = table.Column<string>(type: "text", nullable: false),
                    PatientName = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MakerId = table.Column<string>(type: "text", nullable: true),
                    MakerRecommendation = table.Column<int>(type: "integer", nullable: true),
                    MakerFeedback = table.Column<string>(type: "text", nullable: true),
                    CheckerId = table.Column<string>(type: "text", nullable: true),
                    CheckerDecision = table.Column<int>(type: "integer", nullable: true),
                    CheckerFeedback = table.Column<string>(type: "text", nullable: true),
                    DateForwarded = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Claims");
        }
    }
}
