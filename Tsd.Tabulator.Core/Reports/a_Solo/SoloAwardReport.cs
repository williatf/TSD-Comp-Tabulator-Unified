using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Core.Reports.a_Solo
{
    public class SoloAwardReport
    {
        public SoloAwardReport(IReadOnlyList<SoloAwardGroup> groups)
        {
            Groups = groups;
        }

        public IReadOnlyList<SoloAwardGroup> Groups { get; }
    }
}
