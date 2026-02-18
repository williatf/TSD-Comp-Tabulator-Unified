namespace Tsd.Tabulator.Core.Reporting;

public sealed class BucketGroup
{
    public string Bucket { get; }
    public List<ClassGroup> Classes { get; }

    public BucketGroup(string bucket, List<ClassGroup> classes)
    {
        Bucket = bucket;
        Classes = classes;
    }
}