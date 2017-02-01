using System;
using System.IO;
using SolutionRenamer.Win.Logic.FileSystem;

namespace SolutionRenamer.Win.Logic
{
    public class RenamerRule
    {
        private readonly Func<RenamerInfo, IFileSystem, bool> _rule;

        public RenamerRule(string name, Func<RenamerInfo, IFileSystem, bool> rule)
        {
            _rule = rule;
        }

        public static RenamerRule IsNotBinOrObj
        {
            get
            {
                return new RenamerRule("Not bin or obj", (r, f) =>
                {
                    return !DirectoryOrFileStartsWith(r.Path, "bin") &&
                        !DirectoryOrFileStartsWith(r.Path, "obj");
                });
            }
        }

        public static RenamerRule IsNotCache
        {
            get
            {
                return new RenamerRule("Not .cache", (r, f) =>
                {
                    return !FileExtensionMatches(r.Path, "cache");
                });
            }
        }

        public static RenamerRule IsNotDbmdl
        {
            get
            {
                return new RenamerRule("Not DBMDL", (r, f) =>
                {
                    return !FileExtensionMatches(r.Path, "dbmdl");
                });
            }
        }

        public static RenamerRule IsNotDll
        {
            get
            {
                return new RenamerRule("Not DLL", (r, f) =>
                {
                    return !FileExtensionMatches(r.Path, "dll") &&
                        !FileExtensionMatches(r.Path, "exe");
                });
            }
        }

        public static RenamerRule IsNotImage
        {
            get
            {
                return new RenamerRule("Not Image", (r, f) =>
                {
                    return !FileExtensionMatches(r.Path, "png") &&
                        !FileExtensionMatches(r.Path, "gif") &&
                        !FileExtensionMatches(r.Path, "jpg");
                });
            }
        }

        public static RenamerRule IsNotGit
        {
            get
            {
                return new RenamerRule("Not Git", (r, f) =>
                {
                    return !DirectoryOrFileStartsWith(r.Path, ".git");
                });
            }
        }

        public static RenamerRule IsNotNuget
        {
            get
            {
                return new RenamerRule("Not Nuget", (r, f) =>
                {
                    return !DirectoryOrFileStartsWith(r.Path, "packages");
                });
            }
        }

        public static RenamerRule IsNotPdb
        {
            get
            {
                return new RenamerRule("Not PDB", (r, f) =>
                {
                    return !FileExtensionMatches(r.Path, "pdb");
                });
            }
        }

        public static RenamerRule IsNotSuo
        {
            get
            {
                return new RenamerRule("Not SUO", (r, f) =>
                {
                    return !FileExtensionMatches(r.Path, "suo");
                });
            }
        }

        public static RenamerRule IsTrue
        {
            get
            {
                return new RenamerRule("True", (r, f) =>
                {
                    return true;
                });
            }
        }

        public RenamerRule Next { get; set; }

        public static bool DirectoryOrFileStartsWith(string path, string pattern)
        {
            return path.ToLower().Contains($"\\{pattern.ToLower().TrimStart('\\')}");
        }

        public static bool FileExtensionMatches(string path, string extension)
        {
            return Path.GetExtension(path).ToLower().Contains($".{extension.ToLower().TrimStart('.')}");
        }

        public bool IsSatisfiedBy(RenamerInfo info, IFileSystem fileSystem)
        {
            if (Next != null)
            {
                return Next.IsSatisfiedBy(info, fileSystem) && _rule.Invoke(info, fileSystem);
            }

            return _rule.Invoke(info, fileSystem);
        }
    }
}