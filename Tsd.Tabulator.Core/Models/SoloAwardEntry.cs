using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Models;

/// <summary>
/// Represents a ranked solo award entry with place.
/// </summary>
/// 
public sealed record SoloAwardEntry : AwardEntryBase
{
    public SoloAwardEntry(
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
