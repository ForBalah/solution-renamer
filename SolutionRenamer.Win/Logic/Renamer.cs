using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SolutionRenamer.Win.Logic.Events;
using SolutionRenamer.Win.Logic.FileSystem;
using SolutionRenamer.Win.Logic.Logging;

namespace SolutionRenamer.Win.Logic
{
    public class Renamer
    {
        private readonly ILogger _logger;
        private IFileSystem _fileSystem;
        private RenamerRule _ruleSet;

        public Renamer(ILogger logger, IFileSystem fileSystem, RenamerRule ruleSet)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            _logger = logger;
            _fileSystem = fileSystem;
            _ruleSet = ruleSet;
        }

        public event EventHandler<RenamesSetEventArgs> RenamesSet;

        public event EventHandler<TargetFolderAddedEventArgs> TargetFolderAdded;

        public event EventHandler<SolutionRenamedEventArgs> SolutionRenamed;

        public string NewName { get; private set; }

        public bool Completed { get; private set; }

        public bool Ready
        {
            get
            {
                return !string.IsNullOrEmpty(OldName) && !string.IsNullOrEmpty(NewName) && Root != null && !string.IsNullOrEmpty(TargetFolder);
            }
        }

        public string OldName { get; private set; }

        public RenamerInfo Root { get; private set; }

        public string TargetFolder
        {
            get
            {
                return Root?.Path;
            }
        }

        public static string ValidateSolutionName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Null or whitespace names are not allowed.";
            }

            if (new Regex(@"^\d+").IsMatch(name))
            {
                return "Names cannot start with a number.";
            }

            if (new Regex(@"\s").IsMatch(name))
            {
                return "Spaces are not allowed.";
            }

            if (new Regex(@"(^\.)|(\.$)|(\.{2,})").IsMatch(name))
            {
                return "Dots cannot be at the beginning, end, or adjacent to each other.";
            }

            if (new Regex(@"[^a-zA-Z0-9_\.]").IsMatch(name))
            {
                return "Only a-Z, 0-9, _ and . are allowed in the name.";
            }

            var splitNames = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            foreach (var part in splitNames)
            {
                if (Config.ReservedWords.Contains(part))
                {
                    return "Reserved words in c# cannot be used as a solution name.";
                }
            }

            return null;
        }

        public void DoRename(IProgress<RenamerProgress> progressReport)
        {
            if (Completed)
            {
                throw new InvalidOperationException("Cannot do a rename on the same process. Select the target folder again.");
            }

            progressReport.Report(new RenamerProgress("=== Starting rename process ===", 0, _logger));
            progressReport.Report(new RenamerProgress("Backing up solution folder to zip", 0.1, _logger));
            var backupResult = _fileSystem.Zip(Root, OldName);
            if (backupResult.IsSuccess)
            {
                progressReport.Report(new RenamerProgress(backupResult.Message, 0.3, _logger));
            }
            else
            {
                progressReport.Report(new RenamerProgress(backupResult.Message, 1, _logger, true));
                Completed = false;
                return;
            }

            Task.Delay(10).Wait();
            _fileSystem.Rename(Root, OldName, NewName, progressReport, _logger);

            // then call rename on the renamerinfo, passing in the naming strategy (or factory)
            progressReport.Report(new RenamerProgress("=== Finished rename process ===", 1, _logger));
            Completed = true;

            if (SolutionRenamed != null)
            {
                SolutionRenamed(this, new SolutionRenamedEventArgs());
            }
        }

        public void SetRename(string oldName, string newName)
        {
            var fromValidationMessage = ValidateSolutionName(oldName);
            var toValidationMessage = ValidateSolutionName(newName);

            if (!string.IsNullOrWhiteSpace(oldName) && !string.IsNullOrWhiteSpace(fromValidationMessage))
            {
                _logger.WriteWarning($"'From' solution name is invalid: {fromValidationMessage}");
            }
            else if (!string.IsNullOrWhiteSpace(newName) && !string.IsNullOrWhiteSpace(toValidationMessage))
            {
                _logger.WriteWarning($"'To' solution name is invalid: {toValidationMessage}");
            }
            else if (!string.IsNullOrWhiteSpace(oldName) && !string.IsNullOrWhiteSpace(newName))
            {
                _logger.WriteInfo($"solution names are valid.");
            }

            OldName = oldName;
            NewName = newName;

            if (RenamesSet != null)
            {
                RenamesSet(this, new RenamesSetEventArgs(string.IsNullOrWhiteSpace(fromValidationMessage), string.IsNullOrWhiteSpace(toValidationMessage)));
            }
        }

        public void SetTargetFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                _logger.WriteWarning($"Invalid folder selected. Folder: {folder ?? "[empty]"}");
                return;
            }

            if (!_fileSystem.IsValidFolder(folder))
            {
                _logger.WriteWarning($"'{folder}' is not a valid folder.");
                return;
            }

            Root = _fileSystem.GetFiles(folder, _ruleSet);
            _logger.WriteInfo($"'{folder}' folder selected.");
            Completed = false;

            if (!_fileSystem.IsSolutionFolder(Root))
            {
                _logger.WriteWarning("WARNING: no solution or more than one solution was found in this folder.");
            }

            if (TargetFolderAdded != null)
            {
                TargetFolderAdded(this, new TargetFolderAddedEventArgs(folder, _fileSystem.IsSolutionFolder(Root)));
            }
        }

        public bool CleanFolders(string folderName)
        {
            var success = true;
            if (Root == null)
            {
                _logger.WriteWarning("Target folder is not set!");
                success = false;
            }
            else
            {
                // We need to reset the target folder first before the clean can commence
                SetTargetFolder(Root.Path);
                var results = new List<RenameResult>();
                _fileSystem.CleanFolders(Root, folderName, results);
                foreach (var result in results)
                {
                    if (result.IsSuccess)
                    {
                        _logger.WriteInfo(result.Message);
                    }
                    else
                    {
                        _logger.WriteWarning(result.Message);
                    }
                }

                if (results.Any(r => !r.IsSuccess))
                {
                    _logger.WriteWarning("Clean was not sucecssful.");
                    success = false;
                }
            }

            return success;
        }
    }
}