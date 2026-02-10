using System.ComponentModel;
using System.Globalization;

namespace Tsd.Tabulator.Wpf.ViewModels.Scoring;

public sealed class ScoreCellVM : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly Action _onChanged;

    public ScoreCellVM(string key, decimal max, Action onChanged)
    {
        Key = key;
        Max = max;
        _onChanged = onChanged;
    }

    public string Key { get; }
    public decimal Max { get; }

    private decimal? _value;
    public decimal? Value
    {
        get => _value;
        private set
        {
            if (_value == value) return;
            _value = value;
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(Text));
            _onChanged();
        }
    }

    // Bind TextBox.Text to this (lets empty string be valid)
    private string _text = "";
    public string Text
    {
        get => _text;
        set
        {
            if (_text == value) return;
            _text = value ?? "";
            ValidateAndSetValue(_text);
            OnPropertyChanged(nameof(Text));
        }
    }

    // --- Validation (INotifyDataErrorInfo) ---
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public System.Collections.IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return _errors.SelectMany(kvp => kvp.Value);

        return _errors.TryGetValue(propertyName, out var list) ? list : Enumerable.Empty<string>();
    }

    private void SetError(string propertyName, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            if (_errors.Remove(propertyName))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            return;
        }

        _errors[propertyName] = new List<string> { message };
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ValidateAndSetValue(string raw)
    {
        raw = raw.Trim();

        // Allow blank
        if (raw.Length == 0)
        {
            SetError(nameof(Text), null);
            Value = null;
            return;
        }

        if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.CurrentCulture, out var parsed))
        {
            SetError(nameof(Text), "Not a number.");
            Value = null;
            return;
        }

        if (parsed < 0)
        {
            SetError(nameof(Text), "Min is 0.");
            Value = null;
            return;
        }

        if (parsed > Max)
        {
            SetError(nameof(Text), $"Max is {Max}!");
            Value = null;
            return;
        }

        SetError(nameof(Text), null);
        Value = parsed;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
