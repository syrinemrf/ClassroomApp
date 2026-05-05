using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubjectId",
                table: "Assignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubjectClassrooms",
                columns: table => new
                {
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectClassrooms", x => new { x.SubjectId, x.ClassroomId });
                    table.ForeignKey(
                        name: "FK_SubjectClassrooms_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectClassrooms_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SubjectId",
                table: "Courses",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_SubjectId",
                table: "Assignments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectClassrooms_ClassroomId",
                table: "SubjectClassrooms",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_TeacherId",
                table: "Subjects",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Subjects_SubjectId",
                table: "Assignments",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Subjects_SubjectId",
                table: "Courses",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Subjects_SubjectId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Subjects_SubjectId",
                table: "Courses");

            migrationBuilder.DropTable(
                name: "SubjectClassrooms");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Courses_SubjectId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_SubjectId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Assignments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$L8bZZ6JJsK.HQ0SNAgHYCus3ZYdQuBcTT25mRTeX6DY7X40jgs4W.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222221"),
                column: "PasswordHash",
                value: "$2a$11$JM3AiY66jofjzk3wSWbWUucntlBnbwFVgZPVlL2W.kebE.9K6IrAG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333331"),
                column: "PasswordHash",
                value: "$2a$11$nOLqRwG1r8A2cl5OdmBdpeMoQv1J5paJdg0o5rvDr6wvPOtYhwIS2");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "PasswordHash",
                value: "$2a$11$M8VY4pbuR/o0cI7CqNTmEuEOj3ea6UpJB0Cd0MaJGzqGr2WO4/oee");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "PasswordHash",
                value: "$2a$11$oPWLGtsSHfuzBeCTzGrxm.63eVJ8ZI.qabluUHIngUnqxKNqQJgYa");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "PasswordHash",
                value: "$2a$11$DRd5yThI5wzMs1kZxW7puuz2QIVwOopEJgQevR/yI318mhiR7UOtu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "PasswordHash",
                value: "$2a$11$OI5FgahlbAzcFizhH7gWceU5IIBYkMcF7yy8NxWAMUd1QnECsUWtm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$yCWdTLRWHZwk1KC0sY9cbeLav9tNSqLvsF3O6aGbSVzT5fY8sQ3ri");
        }
    }
}
