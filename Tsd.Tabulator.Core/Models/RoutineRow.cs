namespace Tsd.Tabulator.Core.Models;

public sealed record RoutineRow(
    string RoutineId,
    string StartTime,
    long ProgramNumber,
    string EntryType,
    string Category,
    string Class,
    string Participants,
    string StudioName,
    string RoutineTitle
);
