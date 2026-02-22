namespace Tsd.Tabulator.Core.Reports.d_Ensemble;

/// <summary>
/// Represents the top-level grouping for the Ensemble Award Report.
/// This groups Ensemble classes (Middle School, High School, etc.)
/// under a bucket (Studio, School, Select School, Elite School).
/// </summary>
public sealed class EnsembleBucketGroup
{
    /// <summary>
    /// The bucket name (e.g., Studio, School, Select School, Elite School).
    /// </summary>
    public string Bucket { get; }

    /// <summary>
    /// The collection of Ensemble class groups contained in this bucket.
    /// </summary>
    public IReadOnlyList<EnsembleClassGroup> Classes { get; }

    public EnsembleBucketGroup(
        string bucket,
        IReadOnlyList<EnsembleClassGroup> classes)
    {
        Bucket = bucket;
        Classes = classes;
    }
}