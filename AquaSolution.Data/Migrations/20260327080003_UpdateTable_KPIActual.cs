using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AquaSolution.Data.Migrations
{
    public partial class UpdateTable_KPIActual : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop ALL FK liên quan tới column UpdatedBy
            migrationBuilder.Sql(@"
DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql += '
ALTER TABLE [KPI].[tbl_KPIMonthlyActuals] DROP CONSTRAINT [' + fk.name + '];'
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN sys.columns c ON fkc.parent_column_id = c.column_id
WHERE c.name = 'UpdatedBy'
  AND fkc.parent_object_id = OBJECT_ID('[KPI].[tbl_KPIMonthlyActuals]');

EXEC(@sql);
");

            // 2. Drop ALL INDEX liên quan tới column UpdatedBy
            migrationBuilder.Sql(@"
DECLARE @sql NVARCHAR(MAX) = '';

SELECT @sql += '
DROP INDEX [' + i.name + '] ON [KPI].[tbl_KPIMonthlyActuals];'
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
JOIN sys.columns c ON ic.column_id = c.column_id
WHERE c.name = 'UpdatedBy'
  AND i.object_id = OBJECT_ID('[KPI].[tbl_KPIMonthlyActuals]');

EXEC(@sql);
");

            // 3. Drop column UpdatedBy
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = 'UpdatedBy' 
      AND Object_ID = Object_ID('[KPI].[tbl_KPIMonthlyActuals]')
)
BEGIN
    ALTER TABLE [KPI].[tbl_KPIMonthlyActuals]
    DROP COLUMN [UpdatedBy];
END
");

            // 4. Create index CreatedBy (nếu chưa có)
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_tbl_KPIMonthlyActuals_CreatedBy'
      AND object_id = OBJECT_ID('[KPI].[tbl_KPIMonthlyActuals]')
)
BEGIN
    CREATE INDEX [IX_tbl_KPIMonthlyActuals_CreatedBy]
    ON [KPI].[tbl_KPIMonthlyActuals]([CreatedBy]);
END
");

            // 5. Add FK CreatedBy (nếu chưa có)
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_tbl_KPIMonthlyActuals_tbl_Users_CreatedBy'
)
BEGIN
    ALTER TABLE [KPI].[tbl_KPIMonthlyActuals]
    ADD CONSTRAINT [FK_tbl_KPIMonthlyActuals_tbl_Users_CreatedBy]
    FOREIGN KEY ([CreatedBy])
    REFERENCES [Admin].[tbl_Users]([Id])
    ON DELETE NO ACTION;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Drop FK CreatedBy
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_tbl_KPIMonthlyActuals_tbl_Users_CreatedBy'
)
BEGIN
    ALTER TABLE [KPI].[tbl_KPIMonthlyActuals]
    DROP CONSTRAINT [FK_tbl_KPIMonthlyActuals_tbl_Users_CreatedBy];
END
");

            // 2. Drop index CreatedBy
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_tbl_KPIMonthlyActuals_CreatedBy'
      AND object_id = OBJECT_ID('[KPI].[tbl_KPIMonthlyActuals]')
)
BEGIN
    DROP INDEX [IX_tbl_KPIMonthlyActuals_CreatedBy]
    ON [KPI].[tbl_KPIMonthlyActuals];
END
");

            // 3. Add lại column UpdatedBy
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE Name = 'UpdatedBy' 
      AND Object_ID = Object_ID('[KPI].[tbl_KPIMonthlyActuals]')
)
BEGIN
    ALTER TABLE [KPI].[tbl_KPIMonthlyActuals]
    ADD [UpdatedBy] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID();
END
");

            // 4. Create index UpdatedBy
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_tbl_KPIMonthlyActuals_UpdatedBy'
      AND object_id = OBJECT_ID('[KPI].[tbl_KPIMonthlyActuals]')
)
BEGIN
    CREATE INDEX [IX_tbl_KPIMonthlyActuals_UpdatedBy]
    ON [KPI].[tbl_KPIMonthlyActuals]([UpdatedBy]);
END
");

            // 5. Add FK UpdatedBy
            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_tbl_KPIMonthlyActuals_tbl_Users_UpdatedBy'
)
BEGIN
    ALTER TABLE [KPI].[tbl_KPIMonthlyActuals]
    ADD CONSTRAINT [FK_tbl_KPIMonthlyActuals_tbl_Users_UpdatedBy]
    FOREIGN KEY ([UpdatedBy])
    REFERENCES [Admin].[tbl_Users]([Id])
    ON DELETE NO ACTION;
END
");
        }
    }
}