using System.Collections.Generic;
using NUnit.Framework;
using SolutionRenamer.Win.Logic.FileSystem;

namespace SolutionRenamer.Tests.UnitTests
{
    [TestFixture]
    public class RenamerInfoTests
    {
        [Test]
        public void ContainsSolutionFolder_WhenNoChildrenAreASolutionFile_ReturnsFalse()
        {
            var info = new RenamerInfo
            {
                Children = new List<RenamerInfo>
                {
                    new RenamerInfo { Path = "path", FileType = FileType.Other }
                }
            };

            Assert.IsFalse(info.ContainsSolutionFile, "The renamer info is not meant to contain a solution file.");
        }

        [Test]
        public void ContainsSolutionFolder_WhenChildIsASolutionFile_ReturnsTrue()
        {
            var info = new RenamerInfo
            {
                Children = new List<RenamerInfo>
                {
                    new RenamerInfo { Path = "path", FileType = FileType.Solution }
                }
            };

            Assert.IsTrue(info.ContainsSolutionFile, "The renamer info is meant to contain a solution file.");
        }

        [Test]
        public void ContainsSolutionFolder_WhenMultipleSolutionFilesPresent_ReturnsFalse()
        {
            var info = new RenamerInfo
            {
                Children = new List<RenamerInfo>
                {
                    new RenamerInfo { Path = "path1", FileType = FileType.Solution },
                    new RenamerInfo { Path = "path2", FileType = FileType.Solution }
                }
            };

            Assert.IsFalse(info.ContainsSolutionFile, "The renamer info is not meant to contain multiple solution files.");
        }

        [Test]
        public void GetItemCount_WithNoChildren_AndIncludingAllItems_ReturnsCountOfItself()
        {
            var root = new RenamerInfo { IsIncluded = true };

            var result = root.GetItemCount(true);

            Assert.AreEqual(1, result, "The count returned was incorrect.");
        }

        [Test]
        public void GetItemCount_WithChildren_AndIncludingAllItems_ReturnsCountWithChildren()
        {
            var root = new RenamerInfo
            {
                IsIncluded = true,
                Children = new List<RenamerInfo>
                {
                    new RenamerInfo { IsIncluded = true }
                }
            };

            var result = root.GetItemCount(true);

            Assert.AreEqual(2, result, "The count returned was incorrect.");
        }

        [Test]
        public void GetItemCount_WithNoChildren_NotIncludingAllItems_WhenItemNotIncluded_ReturnsZero()
        {
            var root = new RenamerInfo { IsIncluded = false };

            var result = root.GetItemCount(false);

            Assert.AreEqual(0, result, "The count returned was incorrect.");
        }

        [Test]
        public void GetItemCount_WithChildren_NotIncludingAllItems_WhenItemNotIncluded_ReturnsZero()
        {
            var root = new RenamerInfo
            {
                IsIncluded = false,
                Children = new List<RenamerInfo>
                {
                    new RenamerInfo { IsIncluded = true }
                }
            };

            var result = root.GetItemCount(false);

            Assert.AreEqual(0, result, "The count returned was incorrect.");
        }

        [Test]
        public void GetItemCount_WithChildren_NotIncludingAllItems_WhenItemIncluded_ReturnsOneItemCount()
        {
            var root = new RenamerInfo
            {
                IsIncluded = true,
                Children = new List<RenamerInfo>
                {
                    new RenamerInfo { IsIncluded = false }
                }
            };

            var result = root.GetItemCount(false);

            Assert.AreEqual(1, result, "The count returned was incorrect.");
        }

        [Test]
        public void GetItemCount_WithChildren_AndIncludingAllItems_WhenItemNotIncluded_ReturnsZero()
        {
            var root = new RenamerInfo
            {
                IsIncluded = false,
                Children = new List<RenamerInfo>
                {
                    new RenamerInfo { IsIncluded = true }
                }
            };

            var result = root.GetItemCount(true);

            Assert.AreEqual(2, result, "The count returned was incorrect.");
        }

        [Test]
        public void GetItemCount_WithChildren_AndIncludingAllItems_WhenItemIncluded_ReturnsOneItemCount()
        {
            var root = new RenamerInfo
            {
                IsIncluded = true,
                Children = new List<RenamerInfo>
                {
                    new RenamerInfo { IsIncluded = false }
                }
            };

            var result = root.GetItemCount(true);

            Assert.AreEqual(2, result, "The count returned was incorrect.");
        }
    }
}