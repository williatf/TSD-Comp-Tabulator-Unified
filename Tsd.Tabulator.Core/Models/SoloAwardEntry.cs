using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Models;

/// <summary>
/// Represents a ranked solo award entry with place.
/// </summary>
public sealed record SoloAwardEntry(
    int Place,
    double FinalScore,
    long ProgramNumber,
    string Participants,
    string StudioName,
    string RoutineTitle
);
