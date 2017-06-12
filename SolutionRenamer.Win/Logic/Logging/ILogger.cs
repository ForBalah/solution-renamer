using System;

namespace SolutionRenamer.Win.Logic.Logging
{
    public interface ILogger
    {
        void WriteInfo(string message);

        void WriteWarning(string message);

        void WriteWarning(string message, Exception ex);
    }
}