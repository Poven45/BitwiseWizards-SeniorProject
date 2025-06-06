﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustTrade.Data.Migrations.TrustTrade
{
    /// <inheritdoc />
    public partial class AddFinancialNewsModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "ProfilePicture",
                table: "Users",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "FinancialNewsItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TimePublished = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Authors = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OverallSentimentScore = table.Column<float>(type: "real", nullable: true),
                    OverallSentimentLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FetchedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialNewsItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialNewsTickerSentiments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsItemId = table.Column<int>(type: "int", nullable: false),
                    TickerSymbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TickerSentimentScore = table.Column<float>(type: "real", nullable: true),
                    TickerSentimentLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelevanceScore = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialNewsTickerSentiments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialNewsTickerSentiments_FinancialNewsItems_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "FinancialNewsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialNewsTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NewsItemId = table.Column<int>(type: "int", nullable: false),
                    TopicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RelevanceScore = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialNewsTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialNewsTopics_FinancialNewsItems_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "FinancialNewsItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNewsItems_Url",
                table: "FinancialNewsItems",
                column: "Url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNewsTickerSentiments_NewsItemId",
                table: "FinancialNewsTickerSentiments",
                column: "NewsItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialNewsTopics_NewsItemId",
                table: "FinancialNewsTopics",
                column: "NewsItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialNewsTickerSentiments");

            migrationBuilder.DropTable(
                name: "FinancialNewsTopics");

            migrationBuilder.DropTable(
                name: "FinancialNewsItems");

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePicture",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);
        }
    }
}
