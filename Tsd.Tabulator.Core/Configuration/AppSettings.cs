using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Configuration;

/// <summary>
/// Application settings and feature flags.
/// </summary>
public static class AppSettings
{
    /// <summary>
    /// Enable automatic sheet suggestion when LastSheetKey is null.
    /// Currently OFF - manual selection only.
    /// </summary>
    public static bool EnableAutoSuggestSheet { get; set; } = false;
}
