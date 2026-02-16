// BucketGroup.cs
using System.Collections.Generic;

namespace Tsd.Tabulator.Application.Models.Reporting;

public sealed class BucketGroup<T>
{
    public string Bucket { get; }
    public IReadOnlyList<ClassGroup<T>> Classes { get; }

    public BucketGroup(string bucket, IReadOnlyList<ClassGroup<T>> classes)
    {
        Bucket = bucket;
        Classes = classes;
    }
}