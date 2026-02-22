using System.Threading.Tasks;
using Tsd.Tabulator.Core.Reporting;

namespace Tsd.Tabulator.Core.Reporting
{
    public interface IReportTab
    {
        Task RefreshAsync();

        // Add these:
        ReportSchema Schema { get; }
        string DisplayName { get; set; }
    }
}