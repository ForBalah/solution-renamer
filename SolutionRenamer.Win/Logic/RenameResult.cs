namespace SolutionRenamer.Win.Logic
{
    public class RenameResult
    {
        public RenameResult(string message, bool isSuccess)
        {
            Message = message;
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; private set; }

        public string Message { get; private set; }
    }
}