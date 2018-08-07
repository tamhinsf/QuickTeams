using System;
using System.IO;
using System.IO.Compression;

namespace QuickTeams.Utils
{
    public class Files
    {
        public static string CreateArchiveFile(string archiveString, string archiveName, string archivePath)
        {
            var fileWriter = System.IO.File.CreateText(archivePath + "/" + archiveName);
            fileWriter.WriteLine(archiveString);
            fileWriter.Dispose();
            return "";
        }

        public static string CreateArchivePath(string archivePath)
        {

            if (File.Exists(archivePath))
            {
                File.Delete(archivePath);
            }

            if (Directory.Exists(archivePath))
            {
                Directory.Delete(archivePath, true);
                Console.WriteLine("Deleting pre-existing temp directory");
            }

            Directory.CreateDirectory(archivePath);
            Console.WriteLine("Creating archive directory");
            Console.WriteLine("Archive path is " + archivePath);

            return archivePath;
        }

        public static void CleanUpTempDirectoriesAndFiles(string archivePath)
        {
            Console.WriteLine("\n");
            Console.WriteLine("Cleaning up archive directories and files");
            Directory.Delete(archivePath, true);
            File.Delete(archivePath);
            Console.WriteLine("Deleted " + archivePath + " and subdirectories");
        }
    }
}