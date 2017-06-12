using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SolutionRenamer.Win.Logic.FileSystem;

namespace SolutionRenamer.Win.Logic.Strategy
{
    public class FileSystemFileRenameStrategy : RenameStrategyBase
    {
        public override RenameResult Rename(RenamerInfo info, string oldName, string newName)
        {
            var message = new StringBuilder($"{info.Path}: ");
            var renameCount = 0;
            var hasError = false;
            if (info.IsIncluded)
            {
                try
                {
                    // read every line, replacing the oldname with the new name
                    var fileContents = File.ReadAllLines(info.Path);
                    for (int i = 0; i < fileContents.Length; i++)
                    {
                        // TODO: fix up logic to be more precise
                        var line = fileContents[i];
                        renameCount += Regex.Matches(line, oldName).Count;
                        fileContents[i] = Regex.Replace(fileContents[i], oldName, newName, RegexOptions.IgnoreCase);
                    }

                    if (renameCount > 0)
                    {
                        var pluralized = "occurrence" + (renameCount != 1 ? "s" : string.Empty);
                        message.Append($"Replaced {renameCount} {pluralized} of '{oldName}'. ");
                        File.WriteAllLines(info.Path, fileContents);
                    }
                    else
                    {
                        message.Append("Unchanged. ");
                    }
                }
                catch (Exception e)
                {
                    message.Append($"Failed to replace contents. {e.ToString()} ");
                    hasError = true;
                }

                if (!hasError)
                {
                    try
                    {
                        // TODO: centralize?
                        var newFilename = ReplaceFilename(info.Path, oldName, newName);
                        if (CanReplacePath(info.Path, newFilename))
                        {
                            File.Move(info.Path, newFilename);
                            message.Append($"Renamed '{info.Path}' file to '{newFilename}' ");
                            info.Path = newFilename;
                        }
                    }
                    catch (Exception e)
                    {
                        message.Append($"Failed to rename '{info.Path}'. {e.ToString()} ");
                        hasError = true;
                    }
                }
            }
            else
            {
                message.Append("skipped. ");
            }

            return new RenameResult(message.ToString(), !hasError);
        }
    }
}