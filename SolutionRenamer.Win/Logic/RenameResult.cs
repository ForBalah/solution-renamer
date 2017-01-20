using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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