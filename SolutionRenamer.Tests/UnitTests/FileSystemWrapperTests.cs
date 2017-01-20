using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SolutionRenamer.Win.Logic.FileSystem;

namespace SolutionRenamer.Tests.UnitTests
{
    [TestFixture]
    public class FileSystemWrapperTests
    {
        [Test]
        public void GetZipStartPath_GivenADirectoryPath_ReturnsParentDirectoryPath()
        {
            var info = new RenamerInfo { Path = Path.GetTempPath(), FileType = FileType.Directory };
            var expected = Path.GetDirectoryName(Path.GetTempPath()) + "\\";
            var result = FileSystemWrapper.GetZipStartPath(info);

            Assert.AreEqual(expected, result, "The parent directory was supposed to be returned.");
        }

        [Test]
        public void GetZipStartPath_GivenAFile_ReturnsParentDirectoryPath()
        {
            var info = new RenamerInfo { Path = Path.GetTempFileName(), FileType = FileType.Other };

            var result = FileSystemWrapper.GetZipStartPath(info);

            Assert.AreEqual(Path.GetTempPath(), result, "The parent directory was supposed to be returned.");
        }

        [Test]
        [TestCase(@"c:\some path\Some File.zip", @"c:\some path\Some File (1).zip")]
        [TestCase(@"c:\some path\Some File (11).zip", @"c:\some path\Some File (12).zip")]
        [TestCase(@"c:\some path\Some (11) File (11).zip", @"c:\some path\Some (11) File (12).zip")]
        [TestCase(@"c:\some path\Some (11) File.zip", @"c:\some path\Some (12) File.zip")]
        [TestCase(@"c:\some path\Some File (1a).zip", @"c:\some path\Some File (1a) (1).zip")]
        [TestCase(@"c:\some path (11)\Some File.zip", @"c:\some path (11)\Some File (1).zip")]
        public void IncrementFileName_WhenFileIsADuplicatedUsingBrackets_IncrementsValueInBrackets(string input, string expectedOutput)
        {
            var result = FileSystemWrapper.IncrementFileName(input);

            Assert.AreEqual(expectedOutput, result, "The filename was not incremented correctly");
        }
    }
}