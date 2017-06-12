using NUnit.Framework;
using SolutionRenamer.Win.Logic.Strategy;

namespace SolutionRenamer.Tests.UnitTests
{
    [TestFixture]
    public class RenamerStrategyBaseTests
    {
        [Test]
        [TestCase(@"c:\temp\temp.txt", "", "", @"c:\temp\temp.txt")]
        [TestCase(@"c:\Temp\OldFile.txt", "mismatchOldFile", "NewFile", @"c:\Temp\OldFile.txt")]
        [TestCase(@"c:\Temp\OldFolder", "OldFolder", "NewFolder", @"c:\Temp\NewFolder")]
        [TestCase(@"c:\Temp\OldFolder\", "OldFolder", "NewFolder", @"c:\Temp\NewFolder")]
        [TestCase(@"c:\Temp\OldFolder\OldFile.txt", "OldFile", "NewFile", @"c:\Temp\OldFolder\NewFile.txt")]
        [TestCase(@"c:\Temp\oldfolder", "OldFolder", "NewFolder", @"c:\Temp\NewFolder")] // differing case
        [TestCase(@"c:\Temp\OldFolder\oldfile.txt", "OldFile", "NewFile", @"c:\Temp\OldFolder\NewFile.txt")] // differing case
        [TestCase(@"c:\Temp\OldFolder\SubFolder\", "OldFolder", "NewFolder", @"c:\Temp\OldFolder\SubFolder")]
        [TestCase(@"c:\Temp\OldFile\SubFile.txt", "OldFile", "NewFile", @"c:\Temp\OldFile\SubFile.txt")]
        [TestCase(@"c:\Temp\OldFolder\OldFolder\", "OldFolder", "NewFolder", @"c:\Temp\OldFolder\NewFolder")]
        [TestCase(@"c:\Temp\OldFile\OldFile.txt", "OldFile", "NewFile", @"c:\Temp\OldFile\NewFile.txt")]
        [TestCase(@"c:\Temp\OldFile\newfile.txt", "NewFile", "NewFile", @"c:\Temp\OldFile\NewFile.txt")]
        public void ReplaceFilename_ForProvidedOldAndNewNames_MatchesTheExpectedResult(string pathToReplace, string oldFilename, string newFilename, string expectedResult)
        {
            var result = RenameStrategyBase.ReplaceFilename(pathToReplace, oldFilename, newFilename);

            Assert.AreEqual(expectedResult, result, "Replaced filename and path do not match expected result.");
        }

        [Test]
        [TestCase(@"c:\Temp\File", @"c:\Temp\File.txt", false)]
        [TestCase(@"c:\temp\file.TXT", @"c:\TEMP\File.txt", true)]
        [TestCase(@"c:\Temp\File.txt", @"c:\Temp\File.txt", false)] // can't replace exact same file
        [TestCase(@"c:\\\Temp\File.txt", @"c:\Temp\\File.txt", false)] // can't replace exact same file
        [TestCase(@"c:\\\Temp\file.txt", @"c:\Temp\\File.txt", true)]
        [TestCase(@"c:\\\TEMP\file.txt", @"c:\Temp\\file.txt", false)] // can't replace exact same file
        [TestCase(@"c:\\\TempDirectory\file.txt", @"c:\OtherDirectory\\file.txt", true)] // different directory
        public void CanReplacePath_ForProvidedPaths_ReturnsExpectedBooleanResult(string oldPath, string newPath, bool expected)
        {
            var result = RenameStrategyBase.CanReplacePath(oldPath, newPath);

            Assert.AreEqual(expected, result, "The CanReplacePath method did not return the expected result for the old and new paths provided.");
        }

        // TODO: can we move a directory/file to a different case?
    }
}