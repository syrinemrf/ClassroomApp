using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClassroomApp.Migrations
{
    /// <inheritdoc />
    public partial class CourseClassroomManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classrooms_Teachers_TeacherId",
                table: "Classrooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Classrooms_ClassroomId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_ClassroomId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Classrooms_TeacherId",
                table: "Classrooms");

            migrationBuilder.DropColumn(
                name: "ClassroomId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Classrooms");

            migrationBuilder.CreateTable(
                name: "CourseClassrooms",
                columns: table => new
                {
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseClassrooms", x => new { x.CourseId, x.ClassroomId });
                    table.ForeignKey(
                        name: "FK_CourseClassrooms_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseClassrooms_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_CourseClassrooms_ClassroomId",
                table: "CourseClassrooms",
                column: "ClassroomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseClassrooms");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassroomId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "Classrooms",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Classrooms",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444441"),
                column: "TeacherId",
                value: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.UpdateData(
                table: "Classrooms",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444442"),
                column: "TeacherId",
                value: new Guid("33333333-3333-3333-3333-333333333332"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$6yNx2O9CEqLZn7civVTd8.7NMPbKjmS2jzXGCH0o44x1JzGR.Z7Jy");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222221"),
                column: "PasswordHash",
                value: "$2a$11$Vau3OS6ZLExZmKDXblBtXuLfn.svwAqo0Wc30iYabw.HuGYzVCxoO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333331"),
                column: "PasswordHash",
                value: "$2a$11$et9Gs9GO.SsPeo4lo2/FhucyF/NH/OA9J00b0iv3Rb8D2/uwp/i2i");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "PasswordHash",
                value: "$2a$11$dAFKjXiVnF2ftD2RtAe54u1kI2rXS8YNHdo8V63mR1WGICHnNdzj.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "PasswordHash",
                value: "$2a$11$woKAzGgT2lVgtU9RsAjPdO7i5kdPUtdG9bQTQ1xFBGC3G2O.BjyoK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "PasswordHash",
                value: "$2a$11$i8avlbKWB.hgOubPB.4/B.BVxsleZgrO5N.UFn5TLkZBY8hMZ2Ko.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "PasswordHash",
                value: "$2a$11$Z8M.ZKENUgCoKZVxlJ3AHutJ9BQr49E5WCGZqTYuU8LiSOjO6vSvG");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "PasswordHash",
                value: "$2a$11$GQOyN.LLSv7g.obfmXtWCe.ZWtL2LMkfwPKJdDDYD4OFrrgRNleKO");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_ClassroomId",
                table: "Courses",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Classrooms_TeacherId",
                table: "Classrooms",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classrooms_Teachers_TeacherId",
                table: "Classrooms",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Classrooms_ClassroomId",
                table: "Courses",
                column: "ClassroomId",
                principalTable: "Classrooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
