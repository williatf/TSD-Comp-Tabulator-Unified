namespace Tsd.Tabulator.Core.Reports.d_Ensemble;

/// <summary>
/// Represents a fully enriched Ensemble award candidate, containing all
/// classification metadata (Bucket, Level, Division, Type), routine metadata,
/// and final computed score. This mirrors the structure used by Solo, Duet,
/// and Trio candidates, ensuring consistent grouping and UI binding.
/// </summary>
public sealed record EnsembleAwardCandidate
{
    /// <summary>
    /// The unique routine identifier from the scoring system.
    /// </summary>
    public string RoutineId { get; init; } = string.Empty;

    /// <summary>
    /// The bucket assigned to the class definition (e.g., Studio, School,
    /// Select School, Elite School). This is resolved from class metadata.
    /// </summary>
    public string Bucket { get; init; } = string.Empty;

    /// <summary>
    /// The level derived from the class name using the Ensemble scheme
    /// (e.g., Middle School, High School, Junior, Elementary, J.V.).
    /// </summary>
    public string Level { get; init; } = string.Empty;

    /// <summary>
    /// The full class name (e.g., "Middle School Medium (16–30)",
    /// "Select High School Large (26–35)"). This is the division label
    /// displayed in the UI.
    /// </summary>
    public string Division { get; init; } = string.Empty;

    /// <summary>
    /// The Ensemble entry type (e.g., "Ensemble – Small", "Ensemble – Medium",
    /// "Ensemble – Large", "Ensemble – XL"). Derived from the routine entry type.
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// The program number or routine order number, if available.
    /// </summary>
    public long ProgramNumber { get; init; }

    /// <summary>
    /// The list of participants or performer names associated with the routine.
    /// </summary>
    public string Participants { get; init; } = string.Empty;

    /// <summary>
    /// The studio or school name associated with the routine.
    /// </summary>
    public string StudioName { get; init; } = string.Empty;

    /// <summary>
    /// The title of the routine being performed.
    /// </summary>
    public string RoutineTitle { get; init; } = string.Empty;

    /// <summary>
    /// The final computed score for the routine, typically the average of
    /// judge totals or the sum of weighted scoring components.
    /// </summary>
    public double FinalScore { get; init; }
}