using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionRenamer.Win.Logic.FileSystem
{
    public class RenamerInfo
    {
        private string _path;
        private string _previousPath;

        public IList<RenamerInfo> Children { get; set; } = new List<RenamerInfo>();

        public FileType FileType { get; set; }

        public string Path
        {
            get { return _path; }

            set
            {
                _previousPath = _path;
                _path = value;
            }
        }

        public string PreviousPath { get { return _previousPath; } }

        public bool ContainsSolutionFile
        {
            get
            {
                return Children.Count(i => i.FileType == FileType.Solution) == 1;
            }
        }

        public bool IsIncluded { get; set; }

        public override string ToString()
        {
            var excludeMessate = !IsIncluded ? " - excluded" : string.Empty;
            return $"{System.IO.Path.GetFileName(Path)} : {FileType}{excludeMessate}";
        }

        public int GetItemCount(bool includeAll)
        {
            if (!IsIncluded && !includeAll)
            {
                return 0;
            }
            else
            {
                return 1 + Children.Sum(i => i.GetItemCount(includeAll));
            }
        }
    }
}