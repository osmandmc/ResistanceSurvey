using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RS.EF.Data.Migrations
{
    /// <summary>
    /// After EmployerRoleMerge: the change history's İşveren Durumu rows that name the removed roles Yüklenici
    /// (Contractor, 3) or İşletmeci (Operator, 4), old or new value, are deleted (the user, 2026-11-06: the app starts
    /// fresh for the researchers). Down cannot bring them back.
    /// </summary>
    public partial class RemoveOldEmployerRoleHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [FieldChange]
WHERE [Entity] = 'Resistance' AND [Field] = 'EmployerRole'
  AND ([OldValue] IN ('Contractor', 'Operator', '3', '4') OR [NewValue] IN ('Contractor', 'Operator', '3', '4'));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
