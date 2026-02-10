using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Models
{

/// <summary>
/// Defines the type of competition, which determines available score sheets.
/// </summary>
public enum CompetitionType
{
    /// <summary>Trendsetter Dance scoring</summary>
    TSDance = 0,
    
    /// <summary>USASF-specific scoring</summary>
    USASF = 1,
    
    /// <summary>Oklahoma State-specific scoring</summary>
    OKState = 2
}

}
