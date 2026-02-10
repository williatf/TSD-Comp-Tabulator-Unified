using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Models;

/// <summary>
/// Defines the display order for competition classes.
/// </summary>
public static class ClassOrder
{
    /// <summary>
    /// Gets the sort order for a class name. Lower numbers appear first.
    /// </summary>
    public static int GetOrder(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
            return int.MaxValue;

        // Normalize for comparison
        var normalized = className.Trim().ToLowerInvariant();

        // Age-based ordering (youngest to oldest)
        if (normalized.Contains("petite")) return 10;
        if (normalized.Contains("mini")) return 20;
        if (normalized.Contains("junior")) return 30;
        if (normalized.Contains("teen")) return 40;
        if (normalized.Contains("senior")) return 50;
        if (normalized.Contains("adult")) return 60;

        // Level-based ordering (if no age match)
        if (normalized.Contains("recreational")) return 100;
        if (normalized.Contains("novice")) return 110;
        if (normalized.Contains("intermediate")) return 120;
        if (normalized.Contains("advanced")) return 130;
        if (normalized.Contains("competitive")) return 140;
        if (normalized.Contains("elite")) return 150;

        // Unknown classes last
        return 1000;
    }

    /// <summary>
    /// Gets the bucket order. Studio first, then School.
    /// </summary>
    public static int GetBucketOrder(string bucket)
    {
        return bucket?.ToLowerInvariant() switch
        {
            "studio" => 1,
            "school" => 2,
            _ => 999
        };
    }
}
