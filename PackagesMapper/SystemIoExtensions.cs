using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace PackagesMapper
{
    public static class SystemIoExtensions
    {
        public static DirectoryInfo NavigateTowards(this DirectoryInfo source, DirectoryInfo target)
        {
            var currentDir = target;
            if (source.Is(target))
                return source;

            while (!currentDir.Parent.Is(source))
            {
                currentDir = currentDir.Parent;
                if (currentDir == null)
                    throw new Exception("Moved past target!");
            }

            return currentDir;
        }

        public static DirectoryInfo NavigateTowards(this DirectoryInfo source, FileInfo target)
        {
            return source.NavigateTowards(target.Directory);
        }

        public static bool Is(this DirectoryInfo dirA, DirectoryInfo dirB)
        {
            var a = dirA.Parent != null ? Path.Combine(dirA.Parent.FullName, dirA.Name) : dirA.FullName;
            var b = dirB.Parent != null ? Path.Combine(dirB.Parent.FullName, dirB.Name) : dirB.FullName;

            return a.Equals(b, StringComparison.OrdinalIgnoreCase);
        }

        public static bool Is(this FileInfo fileA, FileInfo fileB)
        {
            var a = fileA.Directory != null ? Path.Combine(fileA.Directory.FullName, fileA.Name) : fileA.FullName;
            var b = fileB.Directory != null ? Path.Combine(fileB.Directory.FullName, fileB.Name) : fileB.FullName;

            return a.Equals(b, StringComparison.OrdinalIgnoreCase);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreateSymbolicLink(
            string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        private enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        public static void SymlinkTo(this DirectoryInfo source, DirectoryInfo destination)
        {
            if (source.Exists)
                throw new Exception("Cannot create symlink when source exists");
            var result = CreateSymbolicLink(source.FullName, destination.FullName, SymbolicLink.Directory);

            if (result) return;
            var exception = new Win32Exception(Marshal.GetLastWin32Error());
            throw new Exception("Symlink did not succeed", exception);
        }

        public static void SymlinkTo(this FileInfo source, FileInfo destination)
        {
            if (source.Exists)
                throw new Exception("Cannot create symlink when source exists");
            var result = CreateSymbolicLink(source.FullName, destination.FullName, SymbolicLink.File);

            if (result) return;
            var exception = new Win32Exception(Marshal.GetLastWin32Error());
            throw new Exception("Symlink did not succeed", exception);
        }
    }
}