using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tsd.Tabulator.Application.Interfaces;
using Tsd.Tabulator.Wpf.ViewModels;

namespace Tsd.Tabulator.Wpf.Reports;

public interface IReportSchemeUiBase
{
    Type DataType { get; }
    IReportTab CreateTabDynamic(object loader, IEventContext context);
}