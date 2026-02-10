using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Wpf.ViewModels.Scoring;

/// <summary>
/// Represents a group header spanning multiple columns.
/// </summary>
public sealed class GroupHeaderVM
{
    public GroupHeaderVM(string text, int startIndex, int span)
    {
        Text = text;
        StartIndex = startIndex;
        Span = span;
    }

    /// <summary>Group header text</summary>
    public string Text { get; }
    
    /// <summary>Starting column index (0-based, not including judge label column)</summary>
    public int StartIndex { get; }
    
    /// <summary>Number of columns this header spans</summary>
    public int Span { get; }
}
