using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMicroService.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexForSeriesAndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Backfill SeriesId for recurring tasks that are missing it
            // Group by UserId, Name, StartDate to identify series
            migrationBuilder.Sql(@"
                UPDATE t
                SET t.SeriesId = base.SeriesKey
                FROM Tasks t
                INNER JOIN (
                    SELECT 
                        Id,
                        COALESCE(SeriesId, MIN(Id) OVER (PARTITION BY UserId, Name, StartDate)) AS SeriesKey
                    FROM Tasks
                    WHERE RecurrenceType IS NOT NULL 
                      AND RecurrenceType != 'None'
                      AND RecurrenceType != ''
                ) base ON base.Id = t.Id
                WHERE t.SeriesId IS NULL
                  AND t.RecurrenceType IS NOT NULL 
                  AND t.RecurrenceType != 'None'
                  AND t.RecurrenceType != '';
            ");

            // Step 2: Remove duplicate tasks (keep the one with lowest Id per SeriesId + NextOccurrenceDate)
            migrationBuilder.Sql(@"
                ;WITH Base AS (
                    SELECT *, 
                           COALESCE(SeriesId, Id) AS SeriesKey
                    FROM Tasks
                    WHERE NextOccurrence IS NOT NULL
                ),
                Dups AS (
                    SELECT 
                        Id,
                        SeriesKey,
                        CAST(NextOccurrence AS date) AS NextDate,
                        ROW_NUMBER() OVER (
                            PARTITION BY SeriesKey, CAST(NextOccurrence AS date) 
                            ORDER BY Id ASC
                        ) AS rn
                    FROM Base
                    WHERE NextOccurrence IS NOT NULL
                )
                DELETE FROM Tasks
                WHERE Id IN (SELECT Id FROM Dups WHERE rn > 1);
            ");

            // Step 3: Create computed column for NextOccurrenceDate (date part only)
            migrationBuilder.Sql(@"
                IF COL_LENGTH('Tasks', 'NextOccurrenceDate') IS NULL
                BEGIN
                    ALTER TABLE Tasks 
                    ADD NextOccurrenceDate AS CAST(NextOccurrence AS date) PERSISTED;
                END
            ");

            // Step 4: Create unique index on (SeriesId, NextOccurrenceDate)
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SeriesId_NextOccurrenceDate",
                table: "Tasks",
                columns: new[] { "SeriesId", "NextOccurrenceDate" },
                unique: true,
                filter: "[SeriesId] IS NOT NULL AND [NextOccurrence] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_SeriesId_NextOccurrenceDate",
                table: "Tasks");

            migrationBuilder.Sql(@"
                IF COL_LENGTH('Tasks', 'NextOccurrenceDate') IS NOT NULL
                BEGIN
                    ALTER TABLE Tasks DROP COLUMN NextOccurrenceDate;
                END
            ");
        }
    }
}
