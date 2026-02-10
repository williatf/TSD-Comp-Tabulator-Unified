using System;
using System.Diagnostics;
using Caliburn.Micro;

namespace Tsd.Tabulator.Wpf;

public sealed class DebugLog : ILog
{
    private readonly Type _type;

    public DebugLog(Type type) => _type = type;

    public void Info(string format, params object[] args)
        => Debug.WriteLine($"[{_type.Name}] INFO: {string.Format(format, args)}");

    public void Warn(string format, params object[] args)
        => Debug.WriteLine($"[{_type.Name}] WARN: {string.Format(format, args)}");

    public void Error(Exception exception)
        => Debug.WriteLine($"[{_type.Name}] ERROR: {exception}");
}
