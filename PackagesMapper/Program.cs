using System;
using System.IO;
using System.Linq;

namespace PackagesMapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = Configuration.Parse(args);

            if (config.LinkSource.Length == 0)
                return;

            if (!config.LinkSource.All(d => d.Exists))
                throw new Exception("Some of the source directories does not exist");

            if (!config.LinkDestination.Exists)
            {
                if (config.CreateNewDestination)
                    config.LinkDestination.Create();
                else
                    throw new Exception("Destination dir doesn't exist. Use --create-new-destination to create if missing.");
            }

            Console.WriteLine($@"Mapping {config.LinkSource.Length} directories to {config.LinkDestination.FullName}");

            foreach (var source in config.LinkSource)
            {
                Console.WriteLine("    Source: " + source.FullName);
                if ((source.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    Console.WriteLine("        Skipped: Directory is already mapped");
                    continue;
                }

                foreach (var packageDirectory in source.GetDirectories())
                {
                    Console.Write("        Package: " + packageDirectory.Name);
                    var packageInDestination = new DirectoryInfo(Path.Combine(config.LinkDestination.FullName, packageDirectory.Name));
                    if (!packageInDestination.Exists)
                    {
                        Console.Write(" - Moving to destination: ");
                        try
                        {
                            packageDirectory.MoveTo(packageInDestination.FullName);
                        }
                        catch
                        {
                            Console.WriteLine();
                            throw;
                        }

                        Console.WriteLine("Done!");
                    }
                    else
                    {
                        Console.WriteLine(" - Already exists!");
                        packageDirectory.Delete(true);
                    }
                }

                source.Delete();
                source.Refresh();
                source.SymlinkTo(config.LinkDestination);
            }

            if (config.Pause)
                Console.ReadLine();
        }
    }
}