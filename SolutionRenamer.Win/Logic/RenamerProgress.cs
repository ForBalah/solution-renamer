using SolutionRenamer.Win.Logic.Logging;

namespace SolutionRenamer.Win.Logic
{
    public class RenamerProgress
    {
        public RenamerProgress(string message, double percent, ILogger logger)
        {
            Message = message;
            Percentage = percent;
            Logger = logger;
            IsError = false;
        }

        public RenamerProgress(string message, double percent, ILogger logger, bool isError)
        {
            Message = message;
            Percentage = percent;
            Logger = logger;
            IsError = isError;
        }

        public string Message { get; set; }

        public double Percentage { get; set; }

        public bool IsError { get; set; }

        public ILogger Logger { get; set; }
    }
}