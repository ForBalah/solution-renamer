using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SolutionRenamer.Win.Logic.Logging;
using SolutionRenamer.Win.Logic.Strategy;

namespace SolutionRenamer.Win.Logic.FileSystem
{
    public class FileSystemWrapper : IFileSystem
    {
        private static RenameStrategyBase DirectoryRenameStrategy = new FileSystemDirectoryRenameStrategy();
        private static RenameStrategyBase FileRenameStrategy = new FileSystemFileRenameStrategy();
        private int _currentCount = 0;
        private int _totalCount = 0;

        private FileSystemWrapper()
        {
        }

        public static IFileSystem Create()
        {
            return new FileSystemWrapper();
        }

        public RenamerInfo GetFiles(string folder, RenamerRule ruleSet)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentNullException(nameof(folder));
            }

            var root = new RenamerInfo
            {
                Path = folder,
                FileType = FileType.Directory,
                IsIncluded = true
            };

            RecursiveBuildRenamerInfo(root, ruleSet, true);
            return root;
        }

        public bool IsSolutionFolder(RenamerInfo root)
        {
            return root.ContainsSolutionFile;
        }

        public bool IsValidFolder(string folder)
        {
            return Directory.Exists(folder) && Path.IsPathRooted(folder);
        }

        public void Rename(RenamerInfo root, string oldName, string newName, IProgress<RenamerProgress> progressReport, ILogger logger)
        {
            // the correct thing to do would be to put a lock around here (to prevent multiple threads from hitting this)
            // but since I'm not building this to be multi-threaded, i don't care.
            _totalCount = root.GetItemCount(false);
            _currentCount = 0;

            RecursiveRename(root, oldName, newName, progressReport, logger);
        }

        public RenameResult Zip(RenamerInfo root, string zipName)
        {
            string zipPath = Path.Combine(GetZipStartPath(root), $"{zipName}_{DateTime.Now.ToString("yyyyMMdd")}.zip");

            try
            {
                int tries = 20;
                while (File.Exists(zipPath))
                {
                    zipPath = IncrementFileName(zipPath);
                    if (--tries <= 0)
                    {
                        break;
                    }
                }

                if (root.FileType == FileType.Directory)
                {
                    ZipFile.CreateFromDirectory(root.Path, zipPath);
                }
                else
                {
                    using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(root.Path, Path.GetFileName(root.Path));
                    }
                }

                return new RenameResult(root.Path + " has been backed up successfully", true);
            }
            catch (Exception e)
            {
                return new RenameResult("Backup failed. " + e.Message, false);
            }
        }

        internal static string GetZipStartPath(RenamerInfo info)
        {
            return Path.GetDirectoryName(info.Path) + "\\";
        }

        internal static string IncrementFileName(string originalFileName)
        {
            var version = 1;
            var braceRegex = new Regex(@"\(\d+\)");
            var filename = Path.GetFileNameWithoutExtension(originalFileName);
            var matches = braceRegex.Matches(filename);
            var match = matches.Count > 0 ? matches[matches.Count - 1] : null;
            var newFilename = originalFileName;

            if (match != null)
            {
                version = int.Parse(match.Value.Replace("(", string.Empty).Replace(")", string.Empty));
                version++;
                newFilename = filename
                    .Remove(match.Index, match.Length)
                    .Insert(match.Index, match.Result($"({version})")) + Path.GetExtension(originalFileName);
            }
            else
            {
                newFilename = $"{filename} ({version}).zip";
            }

            return Path.Combine(Path.GetDirectoryName(originalFileName), newFilename);
        }

        private RenameStrategyBase GetRenameStrategy(FileType fileType)
        {
            return fileType == FileType.Directory ? DirectoryRenameStrategy : FileRenameStrategy;
        }

        private void RecursiveBuildRenamerInfo(RenamerInfo parent, RenamerRule ruleSet, bool isRoot)
        {
            // go through each of the children
            // create renamer around each file/folder
            var entries = Directory.EnumerateFileSystemEntries(parent.Path).ToList();
            foreach (var path in entries)
            {
                var fixedPath = path.ToLower();
                var info = new RenamerInfo
                {
                    Path = path,
                    FileType = Directory.Exists(fixedPath) ? FileType.Directory
                        : fixedPath.EndsWith(".sln") ? FileType.Solution
                        : fixedPath.EndsWith(".dll") ? FileType.Library
                        : fixedPath.EndsWith(".exe") ? FileType.Executable
                        : fixedPath.EndsWith(".config") ? FileType.Config
                        : fixedPath.EndsWith(".cs") ? FileType.Code
                        : fixedPath.EndsWith(".csproj") ? FileType.Project
                        : fixedPath.EndsWith(".sqlproj") ? FileType.Project
                        : FileType.Other
                };
                info.IsIncluded = ruleSet.IsSatisfiedBy(info, this);

                parent.Children.Add(info);
            }

            // for speed, only continue if we are in a solution folder
            if (!isRoot || parent.ContainsSolutionFile)
            {
                foreach (var info in parent.Children)
                {
                    if (info.FileType == FileType.Directory)
                    {
                        RecursiveBuildRenamerInfo(info, ruleSet, false);
                    }
                }
            }
        }

        private void RecursiveRename(RenamerInfo root, string oldName, string newName, IProgress<RenamerProgress> progressReport, ILogger logger)
        {
            if (root.IsIncluded)
            {
                // IMPORTANT TO DO DEPTH-FIRST TRAVERSAL!
                // first loop through the children, renaming there
                foreach (var item in root.Children)
                {
                    RecursiveRename(item, oldName, newName, progressReport, logger);
                }

                // _now_ can do the root
                RenameResult result = GetRenameStrategy(root.FileType).Rename(root, oldName, newName);
                progressReport.Report(new RenamerProgress(result.Message, UpTheCount(0.7, 0.3), logger, !result.IsSuccess));
            }
            else
            {
                progressReport.Report(new RenamerProgress($"{root.Path}: Skipped.", UpTheCount(0.7, 0.3), logger, false));
            }

            Task.Delay(1).Wait(); // TODO: find a better way of pumping through the updates to the UI
        }

        private double UpTheCount(double weight, double offset)
        {
            // ¯\_(ツ)_/¯ deal with it.
            return ((double)_currentCount++ / (double)_totalCount) * weight + offset;
        }

        public void CleanFolders(RenamerInfo root, string folderName, IList<RenameResult> results)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (folderName == null)
            {
                throw new ArgumentNullException(nameof(folderName));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (root.FileType == FileType.Directory && Path.GetFileName(root.Path).Equals(folderName, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(root.Path);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    root.Children.Clear();
                    results.Add(new RenameResult($"Cleaned directory {root.Path}", true));
                }
                catch (Exception e)
                {
                    results.Add(new RenameResult($"Could not clean {root.Path}. {e.ToString()}", false));
                }
            }
            else
            {
                foreach (var child in root.Children)
                {
                    CleanFolders(child, folderName, results);
                }
            }
        }
    }
}