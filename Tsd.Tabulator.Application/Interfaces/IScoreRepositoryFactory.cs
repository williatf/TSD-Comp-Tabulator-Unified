using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tsd.Tabulator.Application.Interfaces
{
    public interface IScoreRepositoryFactory
    {
        IScoreRepository Create();
    }
}
