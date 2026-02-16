using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Wpf;

/// <summary>
/// Mutable event context updated by ShellViewModel when events load or settings change.
/// </summary>
public sealed class EventContext : IEventContext
{
    public IClassConfigService ClassConfigService { get; }

    public CompetitionType CompetitionType { get; private set; } = CompetitionType.OKState;
    public string? EventDbPath { get; private set; }
    public bool HasEventLoaded => !string.IsNullOrWhiteSpace(EventDbPath);

    public EventContext(IClassConfigService classConfigService)
    {
        ClassConfigService = classConfigService;
    }

    public void UpdateCompetitionType(CompetitionType type)
    {
        CompetitionType = type;
    }

    public void UpdateEventDbPath(string path)
    {
        EventDbPath = path;
    }
}