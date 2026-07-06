using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonelYonetim.Migrations
{
    /// <inheritdoc />
    public partial class IlkOlusturma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Personeller",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdSoyad = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Departman = table.Column<string>(type: "TEXT", nullable: false),
                    Pozisyon = table.Column<string>(type: "TEXT", nullable: false),
                    Eposta = table.Column<string>(type: "TEXT", nullable: false),
                    Maas = table.Column<decimal>(type: "TEXT", nullable: false),
                    IseGirisTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AktifMi = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Personeller");
        }
    }
}
