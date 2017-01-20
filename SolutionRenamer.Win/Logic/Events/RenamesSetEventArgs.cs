using System;

namespace SolutionRenamer.Win.Logic.Events
{
    public class RenamesSetEventArgs : EventArgs
    {
        public RenamesSetEventArgs(bool isFromValid, bool isToValid)
        {
            IsFromValid = isFromValid;
            IsToValid = isToValid;
        }

        public bool IsFromValid { get; private set; }

        public bool IsToValid { get; private set; }
    }
}