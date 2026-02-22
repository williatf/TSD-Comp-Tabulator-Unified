using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tsd.Tabulator.Core.Reporting;
using Tsd.Tabulator.Core.Reports.d_Ensemble;

namespace Tsd.Tabulator.Wpf.Converters;
public sealed class ReportTemplateSelector : DataTemplateSelector
{
    public DataTemplate? StandardTemplate { get; set; }
    public DataTemplate? EnsembleTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is EnsembleBucketGroup)
            return EnsembleTemplate!;

        if (item is BucketGroup)
            return StandardTemplate!;

        return base.SelectTemplate(item, container);
    }

}
