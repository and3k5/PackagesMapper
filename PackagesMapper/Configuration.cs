using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PackagesMapper
{
    public class Configuration
    {
        /// <summary>
        /// The actual real folder containing all dependencies.
        /// </summary>
        public DirectoryInfo LinkDestination { get; private set; }

        /// <summary>
        /// The "fake" folder
        /// </summary>
        public DirectoryInfo[] LinkSource { get; private set; }

        /// <summary>
        /// Pause when done
        /// </summary>
        public bool Pause { get; private set; }

        /// <summary>
        /// Create new directory if it doesnt exist
        /// </summary>
        public bool CreateNewDestination { get; private set; }

        public static Configuration Parse(string[] args)
        {
            var list = new List<string>(args);

            var result = new Configuration();

            var destination = list[list.IndexOf("--destination-dir") + 1];
            var createNewDestination = list.IndexOf("--create-new-destination") != -1;

            var pause = list.IndexOf("--pause") != -1;

            var sources = GetSourceDirectories(list);
            result.LinkSource = sources;

            result.LinkDestination = new DirectoryInfo(destination);
            result.CreateNewDestination = createNewDestination;
            result.Pause = pause;

            return result;
        }

        private static DirectoryInfo[] GetSourceDirectories(IList<string> list)
        {
            if (list.IndexOf("--scan-dir") < 0)
                throw new Exception("No --source-dir provided!");

            var scanDir = list[list.IndexOf("--scan-dir") + 1];
            var packagesDirs = new DirectoryInfo(scanDir);
            var excludeRootPackageDirectories = list.IndexOf("--ignore-root-package-directories") != -1;
            var scanner = new DirScanner(packagesDirs, excludeRootPackageDirectories);

            var scanDirTask = Task.Run(() =>
            {
                scanner.Work();
                return scanner.Result;
            });

            var sources = scanDirTask.Result.ToArray();
            return sources;
        }

        private class DirScanner
        {
            private readonly DirectoryInfo _directory;
            private readonly bool _excludeRoot;

            public DirScanner(DirectoryInfo directory, bool excludeRoot)
            {
                _directory = directory;
                _excludeRoot = excludeRoot;
            }

            public readonly List<DirectoryInfo> Result = new List<DirectoryInfo>();

            public void Work()
            {
                var currentDirectory = _directory;

                IterateDirectories(currentDirectory, true);
            }

            private void IterateDirectories(DirectoryInfo currentDirectory, bool isRoot)
            {
                var directories = currentDirectory.GetDirectories();
                if (!isRoot || !_excludeRoot)
                {
                    var rootResult = directories.SingleOrDefault(dir => dir.Name.Equals("packages", StringComparison.OrdinalIgnoreCase));
                    if (rootResult != null)
                        Result.Add(rootResult);
                }

                var scanDirs = directories.Where(dir => !dir.Name.Equals("packages", StringComparison.OrdinalIgnoreCase)).ToArray();

                foreach (var scanDir in scanDirs)
                    IterateDirectories(scanDir, false);
            }
        }
    }
}