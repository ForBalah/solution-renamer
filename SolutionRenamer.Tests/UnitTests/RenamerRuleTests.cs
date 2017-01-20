using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SolutionRenamer.Win.Logic;
using SolutionRenamer.Win.Logic.FileSystem;

namespace SolutionRenamer.Tests.UnitTests
{
    [TestFixture]
    public class RenamerRuleTests
    {
        #region IsSatisfiedBy

        [Test]
        public void IsSatisfiedBy_ForSingleRule_WithTrue_ReturnsTrue()
        {
            var info = new RenamerInfo();
            var fileSystem = new Mock<IFileSystem>().Object;
            var rule1 = new RenamerRule("rule 1", (a, b) => true);

            var result = rule1.IsSatisfiedBy(info, fileSystem);

            Assert.IsTrue(result, "True rule is meant to be true");
        }

        [Test]
        public void IsSatisfiedBy_ForSingleRule_WithFalse_ReturnsFalse()
        {
            var info = new RenamerInfo();
            var fileSystem = new Mock<IFileSystem>().Object;
            var rule1 = new RenamerRule("rule 1", (a, b) => false);

            var result = rule1.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "False rule is meant to be false");
        }

        [Test]
        public void IsSatisfiedBy_ForMultipleRules_WithFalseAndFalse_ReturnsFalse()
        {
            var info = new RenamerInfo();
            var fileSystem = new Mock<IFileSystem>().Object;
            var rule1 = new RenamerRule("rule 1", (a, b) => false);
            var rule2 = new RenamerRule("rule 2", (a, b) => false);
            rule1.Next = rule2;

            var result = rule1.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "False and false rules are meant to be false");
        }

        [Test]
        public void IsSatisfiedBy_ForMultipleRules_WithFalseAndTrue_ReturnsFalse()
        {
            var info = new RenamerInfo();
            var fileSystem = new Mock<IFileSystem>().Object;
            var rule1 = new RenamerRule("rule 1", (a, b) => false);
            var rule2 = new RenamerRule("rule 2", (a, b) => true);
            rule1.Next = rule2;

            var result = rule1.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "False and true rules are meant to be false");
        }

        [Test]
        public void IsSatisfiedBy_ForMultipleRules_WithTrueAndFalse_ReturnsFalse()
        {
            var info = new RenamerInfo();
            var fileSystem = new Mock<IFileSystem>().Object;
            var rule1 = new RenamerRule("rule 1", (a, b) => true);
            var rule2 = new RenamerRule("rule 2", (a, b) => false);
            rule1.Next = rule2;

            var result = rule1.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "True and false rules are meant to be false");
        }

        [Test]
        public void IsSatisfiedBy_ForMultipleRules_WithTrueAndTrue_ReturnsTrue()
        {
            var info = new RenamerInfo();
            var fileSystem = new Mock<IFileSystem>().Object;
            var rule1 = new RenamerRule("rule 1", (a, b) => true);
            var rule2 = new RenamerRule("rule 2", (a, b) => true);
            rule1.Next = rule2;

            var result = rule1.IsSatisfiedBy(info, fileSystem);

            Assert.IsTrue(result, "True and true rules are meant to be true");
        }

        #endregion IsSatisfiedBy

        [Test]
        public void True_EnsureDefaultRuleAlwaysReturnsTrue()
        {
            var info = new RenamerInfo { };
            var fileSystem = new Mock<IFileSystem>().Object;

            var result = RenamerRule.IsTrue.IsSatisfiedBy(info, fileSystem);

            Assert.IsTrue(result, "This rule is always meant to return true.");
        }

        [Test]
        public void NotGit_PathsWithGitShouldReturnFalse()
        {
            var info = new RenamerInfo { Path = @"c:\.git\" };
            var fileSystem = new Mock<IFileSystem>().Object;

            var result = RenamerRule.IsNotGit.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "Expected .Git match to return false.");
        }

        [Test]
        public void NotDll_PathsWithDllShouldReturnFalse()
        {
            var info = new RenamerInfo { Path = @"c:\asdf.dll" };
            var fileSystem = new Mock<IFileSystem>().Object;

            var result = RenamerRule.IsNotDll.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "Expected .dll match to return false.");
        }

        [Test]
        public void NotDll_PathsWithExeShouldReturnFalse()
        {
            var info = new RenamerInfo { Path = @"c:\asdf.exe" };
            var fileSystem = new Mock<IFileSystem>().Object;

            var result = RenamerRule.IsNotDll.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "Expected .exe match to return false.");
        }

        [Test]
        public void NotBinOrObj_PathsWithBinShouldReturnFalse()
        {
            var info = new RenamerInfo { Path = @"c:\bin\asdf.exe" };
            var fileSystem = new Mock<IFileSystem>().Object;

            var result = RenamerRule.IsNotDll.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "Expected bin match to return false.");
        }

        [Test]
        public void NotBinOrObj_PathsWithObjShouldReturnFalse()
        {
            var info = new RenamerInfo { Path = @"c:\obj\asdf.exe" };
            var fileSystem = new Mock<IFileSystem>().Object;

            var result = RenamerRule.IsNotDll.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "Expected obj match to return false.");
        }

        [Test]
        public void NotBinOrObj_PathsWithPackagesShouldReturnFalse()
        {
            var info = new RenamerInfo { Path = @"c:\packages\asdf.exe" };
            var fileSystem = new Mock<IFileSystem>().Object;

            var result = RenamerRule.IsNotDll.IsSatisfiedBy(info, fileSystem);

            Assert.IsFalse(result, "Expected obj match to return false.");
        }
    }
}