using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionRenamer.Win.Logic.Events
{
    public class TargetFolderAddedEventArgs : EventArgs
    {
        public TargetFolderAddedEventArgs(string path, bool isValid)
        {
            Path = path;
            IsValid = isValid;
        }

        public string Path { get; private set; }

        public bool IsValid { get; private set; }
    }
}