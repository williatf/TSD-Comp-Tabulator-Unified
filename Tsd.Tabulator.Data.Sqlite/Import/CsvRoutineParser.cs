using CsvHelper;
using CsvHelper.Configuration;
using System.Formats.Asn1;
using System.Globalization;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Data.Sqlite.Import;

public static class CsvRoutineParser
{
    public static IReadOnlyList<CsvRoutineRow> ParseFile(string csvPath)
    {
        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            MissingFieldFound = null
        });

        var rows = new List<CsvRoutineRow>();

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var startTime = csv.GetField("StartTime") ?? "";
            var entryId = csv.GetField("EntryID") ?? "0";
            var entryType = csv.GetField("EntryType") ?? "";
            var category = csv.GetField("Category") ?? "";
            var @class = csv.GetField("Class") ?? "";
            var participants = csv.GetField("Participants") ?? "";
            var studio = csv.GetField("StudioName") ?? "";
            var title = csv.GetField("Routine Title") ?? "";

            if (!int.TryParse(entryId, out var programNumber))
                programNumber = 0;

            rows.Add(new CsvRoutineRow(startTime, programNumber, entryType, category, @class, participants, studio, title));
        }

        return rows;
    }
}
