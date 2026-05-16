using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentFileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Assignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$QSSLj.lnzl8I1gLh4fdmTOtzirha1HmkK4fTugbR14BRKn6saWege");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222221"),
                column: "PasswordHash",
                value: "$2a$11$.zE96PWaaKtbHRM9lzNy6.z.RYaQchfPIVnfchY0SmOHE62YZqTou");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333331"),
                column: "PasswordHash",
                value: "$2a$11$J21H5ShsF.zLl9lpfkfczu8WKorWrbp4k2I7PKEDtQ8KnV4RcsIW2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "PasswordHash",
                value: "$2a$11$N.8v4REWqyL3TnfWuEYsx.2g52lhmIItNItnGuNuG72W/wWCd52DK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "PasswordHash",
                value: "$2a$11$eEQXtDxWjWbn8nmVYadSq.k7T9/yfZM0gbcn3Wsc/KLXO2ROfF8Cu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "PasswordHash",
                value: "$2a$11$bcFiPITEU303ms1JWONn5eEoOVFwXkuUecnAnFmdoFZVEOLNBahuq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "PasswordHash",
                value: "$2a$11$1MlSFaJq6GryLGL7aNPgseXOaVQaCmw5QtIddJJS75AwojaHVPLVm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$Gsq4VCB0tfq8otH9NfQYkuaGv6JKlJpuXpXuov2GK69zB25TtC.H.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Assignments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$XKvV6WX9WjjYztTEqqpn.uGVWk4EA9n9f80sIsC3niHmFMZndOEqK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222221"),
                column: "PasswordHash",
                value: "$2a$11$ZkftPms9e.FX3IU2LFwdJOXC1jN69Jnv1vCd3Q8MrRpiPTpD3CTTa");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333331"),
                column: "PasswordHash",
                value: "$2a$11$1JNcqfqJek/4.adwH3V6HeHypRDcCZsiDVnenfBbLQy7iW5AaDaZe");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "PasswordHash",
                value: "$2a$11$ztjebadCLt594ZAvbVvpeeIQkjGcBNi2EmQsjA/y6M24DkFEqsSaa");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "PasswordHash",
                value: "$2a$11$IRkKAzuRsCUb0C/UeQoxM.MzeOtspjYBPOJp8DTeX6Ku01/vzCLmO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "PasswordHash",
                value: "$2a$11$t9P0AG9K0tDzYflVNOUrH.jbAeKPJVxqqOI64ohrd2ZrxKi.yN8Du");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "PasswordHash",
                value: "$2a$11$vTn7F57EyrA.vk9j3pnfhOs64SqUf8.AZwSNV2b69pm9kjZEPRU8y");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$J3QhxkLopl5uQvkXc5yTUuXXZEw1n8kwi1Ys9D7HdfFnTOcSGVzAC");
        }
    }
}
