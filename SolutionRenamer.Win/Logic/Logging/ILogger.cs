using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionRenamer.Win.Logic.Logging
{
    public interface ILogger
    {
        void WriteInfo(string message);

        void WriteWarning(string message);

        void WriteWarning(string message, Exception ex);
    }
}