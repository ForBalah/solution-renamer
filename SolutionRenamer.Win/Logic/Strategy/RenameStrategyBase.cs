using System;
using System.IO;
using System.Text.RegularExpressions;
using SolutionRenamer.Win.Logic.FileSystem;

namespace SolutionRenamer.Win.Logic.Strategy
{
    public abstract class RenameStrategyBase
    {
        public string LastRenameTask { get; protected set; }

        public abstract RenameResult Rename(RenamerInfo info, string oldName, string newName);

        public static bool CanReplacePath(string oldPath, string newPath)
        {
            var cleanedOldPath = Path.GetFullPath(oldPath);
            var cleanedNewPath = Path.GetFullPath(newPath);

            // filesystem won't let us replace exact same file
            if (Path.GetDirectoryName(cleanedOldPath).Equals(Path.GetDirectoryName(cleanedNewPath), StringComparison.InvariantCultureIgnoreCase) &&
                Path.GetFileNameWithoutExtension(cleanedOldPath) == Path.GetFileNameWithoutExtension(cleanedNewPath))
            {
                return false;
            }

            return cleanedOldPath != cleanedNewPath;
        }

        public static string ReplaceFilename(string pathToReplace, string oldFilename, string newFilename)
        {
            if (string.IsNullOrWhiteSpace(pathToReplace) ||
                string.IsNullOrWhiteSpace(oldFilename) ||
                string.IsNullOrWhiteSpace(newFilename))
            {
                return pathToReplace;
            }

            var cleanedPath = Path.GetFullPath(pathToReplace).TrimEnd('\\');

            var currentPart = Path.GetFileNameWithoutExtension(cleanedPath);
            var newPart = Regex.Replace(currentPart, oldFilename, newFilename, RegexOptions.IgnoreCase);

            return Path.Combine(Path.GetDirectoryName(cleanedPath), newPart + Path.GetExtension(cleanedPath));
        }
    }
}