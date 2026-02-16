using System.Collections.Generic;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Application.Interfaces.Reporting;

public interface IReportSchemeProvider
{
    dynamic? GetScheme(string reportId, CompetitionType type);
    IEnumerable<dynamic> GetSchemesFor(CompetitionType type);
}