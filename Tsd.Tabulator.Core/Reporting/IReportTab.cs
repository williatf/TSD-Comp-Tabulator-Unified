using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Reporting
{
    public interface IReportTab
    {
        Task RefreshAsync();
    }
}
