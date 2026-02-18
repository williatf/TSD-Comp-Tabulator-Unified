namespace Tsd.Tabulator.Core.Models;

public abstract record AwardEntryBase
{
    public int Place { get; init; }
    public double FinalScore { get; init; }
    public long ProgramNumber { get; init; }
    public string Participants { get; init; } = string.Empty;
    public string StudioName { get; init; } = string.Empty;
    public string RoutineTitle { get; init; } = string.Empty;
    public string ClassKey { get; init; } = string.Empty;

    public string PlaceName => AwardPlaceNames.ToPlaceName(Place);
}