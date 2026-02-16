using System;
using System.Collections.Generic;
using System.Linq;
using Tsd.Tabulator.Application.Interfaces.Reporting;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Application.Reports;

public sealed class ReportSchemeProvider : IReportSchemeProvider
{
    // (CompetitionType, ReportId) => scheme
    private readonly Dictionary<(CompetitionType, string), object> _schemes;

    public ReportSchemeProvider(IEnumerable<object> schemes)
    {
        _schemes = new Dictionary<(CompetitionType, string), object>();

        foreach (var scheme in schemes)
        {
            var type = scheme.GetType();

            var supportedTypesProp = type.GetProperty("SupportedTypes");
            var reportIdProp = type.GetProperty("ReportId");

            if (supportedTypesProp == null || reportIdProp == null)
                continue;

            var supportedTypesObj = supportedTypesProp.GetValue(scheme);
            var reportIdObj = reportIdProp.GetValue(scheme);

            if (supportedTypesObj is not IEnumerable<CompetitionType> supportedTypes)
                continue;

            if (reportIdObj is not string reportId)
                continue;

            foreach (var ct in supportedTypes)
                _schemes[(ct, reportId)] = scheme;
        }
    }

    public dynamic? GetScheme(string reportId, CompetitionType type)
    {
        return _schemes.TryGetValue((type, reportId), out var scheme) ? scheme : null;
    }

    public IEnumerable<dynamic> GetSchemesFor(CompetitionType type)
    {
        return _schemes
            .Where(kvp => kvp.Key.Item1 == type)
            .Select(kvp => kvp.Value);
    }
}