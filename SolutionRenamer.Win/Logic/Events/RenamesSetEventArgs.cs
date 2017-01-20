using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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