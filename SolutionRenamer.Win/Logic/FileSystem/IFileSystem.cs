﻿using System;
using System.Collections.Generic;
using SolutionRenamer.Win.Logic.Logging;

namespace SolutionRenamer.Win.Logic.FileSystem
{
    public interface IFileSystem
    {
        RenamerInfo GetFiles(string folder, RenamerRule _ruleSet);

        bool IsValidFolder(string folder);

        bool IsSolutionFolder(RenamerInfo root);

        RenameResult Zip(RenamerInfo root, string zipName);

        void Rename(RenamerInfo root, string oldName, string newName, IProgress<RenamerProgress> progressReport, ILogger logger);

        void CleanFolders(RenamerInfo root, string folderName, IList<RenameResult> results);

        void DeleteFiles(RenamerInfo root, string folderName, string filenameSearch, List<RenameResult> results);
    }
}