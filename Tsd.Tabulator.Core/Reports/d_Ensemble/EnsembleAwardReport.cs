using System.Collections.Generic;

namespace Tsd.Tabulator.Core.Reports.d_Ensemble;

/// <summary>
/// Represents the complete Ensemble Award Report, containing all
/// Bucket → Level → Type → Winner groupings.
/// </summary>
public sealed record EnsembleAwardReport
{
    /// <summary>
    /// The collection of bucket groups (Studio, School, Select School, Elite School),
    /// each containing its levels and type groups.
    /// </summary>
    public IReadOnlyList<EnsembleBucketGroup> Buckets { get; }

    /// <summary>
    /// Creates a new <see cref="EnsembleAwardReport"/> with the specified bucket groups.
    /// </summary>
    public EnsembleAwardReport(IReadOnlyList<EnsembleBucketGroup> buckets)
    {
        Buckets = buckets;
    }
}