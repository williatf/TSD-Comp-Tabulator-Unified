using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Models;

/// <summary>
/// Represents a routine eligible for solo awards after participant de-duplication.
/// </summary>
public sealed record SoloAwardCandidate
{
    /// <summary>
    /// Gets the bucket ("School" or "Studio").
    /// </summary>
    public string Bucket { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the original class name.
    /// </summary>
    public string Class { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the comma-separated participant names.
    /// </summary>
    public string Participants { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the program number for display.
    /// </summary>
    public long ProgramNumber { get; init; }
    
    /// <summary>
    /// Gets the studio name.
    /// </summary>
    public string StudioName { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the routine title.
    /// </summary>
    public string RoutineTitle { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets the average of judge totals.
    /// </summary>
    public double FinalScore { get; init; }
}
