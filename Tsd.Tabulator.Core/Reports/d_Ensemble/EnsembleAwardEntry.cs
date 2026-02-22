using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Core.Reports.d_Ensemble;

/// <summary>
/// Represents a ranked Ensemble award entry with assigned place and
/// Ensemble‑specific metadata. Inherits common award fields from
/// <see cref="AwardEntryBase"/>.
/// </summary>
public sealed record EnsembleAwardEntry : AwardEntryBase
{
    /// <summary>
    /// The Ensemble type (Small, Medium, Large, XL).
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The full class name (division) associated with the routine.
    /// </summary>
    public string Division { get; init; } = string.Empty;

    /// <summary>
    /// Creates a new <see cref="EnsembleAwardEntry"/> with the specified
    /// ranking, score, routine metadata, and Ensemble‑specific fields.
    /// </summary>
    public EnsembleAwardEntry(
        int place,
        double finalScore,
        long programNumber,
        string studioName,
        string routineTitle,
        string classKey,
        string type,
        string division
    )
    {
        Place = place;
        FinalScore = finalScore;
        ProgramNumber = programNumber;
        StudioName = studioName;
        RoutineTitle = routineTitle;
        ClassKey = classKey;

        Type = type;
        Division = division;
    }
}