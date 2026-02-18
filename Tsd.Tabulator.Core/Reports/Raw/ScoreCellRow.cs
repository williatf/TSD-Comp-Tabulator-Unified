using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Reports.Raw;
public sealed record ScoreCellRow
{
    public int JudgeIndex { get; init; }
    public decimal Value { get; init; }
}
