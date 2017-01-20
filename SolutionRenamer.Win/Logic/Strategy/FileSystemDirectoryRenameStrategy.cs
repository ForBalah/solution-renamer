using System;
using System.IO;
using System.Text;
using SolutionRenamer.Win.Logic.FileSystem;

namespace SolutionRenamer.Win.Logic.Strategy
{
    public class FileSystemDirectoryRenameStrategy : RenameStrategyBase
    {
        public override RenameResult Rename(RenamerInfo info, string oldName, string newName)
        {
            var message = new StringBuilder($"{info.Path}: ");
            var hasError = false;

            if (info.IsIncluded)
            {
                try
                {
                    // TODO: centralize?
                    var newDirectoryName = ReplaceFilename(info.Path, oldName, newName);
                    if (CanReplacePath(info.Path, newDirectoryName))
                    {
                        Directory.Move(info.Path, newDirectoryName);
                        message.Append($"Renamed '{info.Path}' directory to '{newDirectoryName}'. ");
                        info.Path = newDirectoryName;
                    }
                    else
                    {
                        message.Append("Directory unchanged.");
                    }
                }
                catch (Exception e)
                {
                    message.Append($"Failed to rename '{info.Path}'. {e.ToString()}. ");
                    hasError = true;
                }
            }
            else
            {
                message.Append("Directory skipped.");
            }

            return new RenameResult(message.ToString(), !hasError);
        }
    }
}