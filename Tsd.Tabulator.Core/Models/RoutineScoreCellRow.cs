namespace Tsd.Tabulator.Core.Models;

public sealed record RoutineScoreCellRow(
    string RoutineId,
    string SheetKey,
    long JudgeIndex,
    string CriterionKey,
    double? Value
);