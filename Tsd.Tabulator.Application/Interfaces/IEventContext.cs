using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Application.Interfaces;

/// <summary>
/// Provides access to the currently loaded event's context without UI dependencies.
/// </summary>
public interface IEventContext
{
    /// <summary>Gets the current competition type.</summary>
    CompetitionType CompetitionType { get; }

    /// <summary>Gets the current event database path, or null if no event is loaded.</summary>
    string? EventDbPath { get; }

    /// <summary>Indicates whether an event is currently loaded.</summary>
    bool HasEventLoaded { get; }

    /// <summary>
    /// Updates the competition type of the current event.
    /// </summary>
    /// <param name="type">The new competition type.</param>
    void UpdateCompetitionType(CompetitionType type);

    /// <summary>
    /// Updates the event database path of the current event.
    /// </summary>
    /// <param name="path">The new event database path.</param>
    void UpdateEventDbPath(string path);

    IClassConfigService ClassConfigService { get; }
}
