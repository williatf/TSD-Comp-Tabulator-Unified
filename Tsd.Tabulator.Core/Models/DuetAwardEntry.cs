using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Models;

/// <summary>
/// Represents a ranked duet award entry with place.
/// </summary>
public sealed record DuetAwardEntry : AwardEntryBase
{
    public DuetAwardEntry(
        int place,
        double finalScore,
        long programNumber,
        string participants,
        string studioName,
        string routineTitle,
        string classKey
    )
    {
        Place = place;
        FinalScore = finalScore;
        ProgramNumber = programNumber;
        Participants = participants;
        StudioName = studioName;
        RoutineTitle = routineTitle;
        ClassKey = classKey;
    }
}
