using System;
using Moq;
using NUnit.Framework;
using SolutionRenamer.Win.Logic;
using SolutionRenamer.Win.Logic.FileSystem;

namespace SolutionRenamer.Tests.UnitTests
{
    [TestFixture]
    public class RenamerTests
    {
        [Test]
        public void Constructor_WhenPassedANullLogger_ThrowsException()
        {
            var fileSystem = new Mock<IFileSystem>();
            Assert.Throws<ArgumentNullException>(() =>
            {
                var renamer = new Renamer(null, fileSystem.Object, null);
            });
        }

        [Test]
        public void Constructor_WhenPassedANullFileSystem_ThrowsException()
        {
            var logger = new Mock<Win.Logic.Logging.ILogger>();
            Assert.Throws<ArgumentNullException>(() =>
            {
                var renamer = new Renamer(logger.Object, null, null);
            });
        }

        [Test]
        public void SetTargetFolder_WhenNullFolderPassedIn_DoesNothingAndLogsError()
        {
            var logger = new Mock<Win.Logic.Logging.ILogger>();
            var fileSystem = new Mock<IFileSystem>();
            var renamer = new Renamer(logger.Object, fileSystem.Object, null);

            renamer.SetTargetFolder(null);

            logger.Verify(l => l.WriteWarning(It.IsAny<string>()), Times.AtLeast(1), "The logger was supposed to write a warning message");
        }

        [Test]
        public void SetTargetFolder_WhenNullFolderPassedIn_DoesNotRaiseSetEvent()
        {
            var logger = new Mock<Win.Logic.Logging.ILogger>();
            var fileSystem = new Mock<IFileSystem>();
            var renamer = new Renamer(logger.Object, fileSystem.Object, null);
            var eventRaised = false;
            renamer.TargetFolderAdded += (s, e) => { eventRaised = true; };

            renamer.SetTargetFolder(null);

            Assert.IsFalse(eventRaised, "The TargetFolerAdded event was not meant to be raised");
        }

        [Test]
        public void SetTargetFolder_WithInvalidFolder_DoesNothingAndLogsError()
        {
            var logger = new Mock<Win.Logic.Logging.ILogger>();
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(f => f.IsValidFolder(It.IsAny<string>())).Returns(false);
            var renamer = new Renamer(logger.Object, fileSystem.Object, null);

            renamer.SetTargetFolder("invalidfolder");

            logger.Verify(l => l.WriteWarning(It.IsAny<string>()), Times.AtLeast(1), "The logger was supposed to write a warning message");
            Assert.IsNull(renamer.TargetFolder, "Target folder was not supposed to be set");
        }

        [Test]
        public void SetTargetFolder_WithValidFolder_SetsFolderAndLogsInfo()
        {
            var logger = new Mock<Win.Logic.Logging.ILogger>();
            var fileSystem = new Mock<IFileSystem>();
            fileSystem.Setup(f => f.IsValidFolder(It.IsAny<string>())).Returns(true);
            fileSystem.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<RenamerRule>())).Returns(new RenamerInfo { Path = "validfolder" });
            var renamer = new Renamer(logger.Object, fileSystem.Object, null);
            var eventRaised = false;
            renamer.TargetFolderAdded += (s, e) => { eventRaised = true; };

            renamer.SetTargetFolder("validfolder");

            Assert.IsNotNull(renamer.TargetFolder, "Target folder was supposed to be set");
            logger.Verify(l => l.WriteInfo(It.IsAny<string>()), Times.AtLeast(1), "The logger was supposed to write an info message");
            Assert.IsTrue(eventRaised, "The TargetFolerAdded event was meant to be raised");
        }

        [Test]
        public void SetTargetFolder_WhenFolderIsNotASolutionFolder_LogsWarningMessage()
        {
            var logger = new Mock<Win.Logic.Logging.ILogger>();
            var fileSystem = new Mock<IFileSystem>();
            var renamer = new Renamer(logger.Object, fileSystem.Object, null);

            renamer.SetTargetFolder("folder");

            logger.Verify(l => l.WriteWarning(It.IsAny<string>()), Times.AtLeast(1), "The logger was supposed to write a warning message");
        }

        [Test]
        [TestCase("", "Empty strings should not be valid.")]
        [TestCase(" ", "spaces should not be valid.")]
        [TestCase("a 1", "spaces should not be valid.")]
        [TestCase("1", "Just numbers should not be valid.")]
        [TestCase("11", "Just numbers should not be valid.")]
        [TestCase("11a", "Should not be able to start with a number.")]
        [TestCase("1a1", "Should not be able to start with a number.")]
        [TestCase(".aa", "Should not accept a dot at the beginning.")]
        [TestCase("a..a", "Should not accept 2 dots together.")]
        [TestCase("aa.", "Should not accept a dot at the end.")]
        [TestCase("asdfASDF!@.#$", "Should not accept special characters.")]
        [TestCase("class", "Should not accept reserved words.")]
        [TestCase("class.char", "Should not accept reserved words.")]
        public void ValidateSolutionName_WhenStringIsInvalid_ReturnsErrorMessage(string input, string message)
        {
            var result = Renamer.ValidateSolutionName(input);
            Assert.IsNotNull(result, message);
        }
    }
}