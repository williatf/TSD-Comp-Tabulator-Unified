namespace Tsd.Tabulator.Wpf.ViewModels.Scoring;

public sealed class ScoreColumnVM
{
    public ScoreColumnVM(string key, string header, decimal? max, bool isTotal = false)
    {
        Key = key;
        Header = header;
        Max = max;
        IsTotal = isTotal;
    }

    public string Key { get; }
    public string Header { get; }
    public decimal? Max { get; }
    public bool IsTotal { get; }
    public string MaxText => Max is null ? "" : $"Max {Max}";
}
