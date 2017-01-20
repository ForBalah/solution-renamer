using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionRenamer.Win.Logic.FileSystem
{
    public enum FileType
    {
        Other = 0,
        Directory,
        Solution,
        Executable,
        Library,
        Config,
        Code,
        Project
    }
}