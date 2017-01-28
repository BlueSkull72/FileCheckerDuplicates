using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;

namespace FileCheckerDuplicates
{
    class Program
    {
        static void Main(string[] args)
        {
            bool tryAgain = false;
            do
            {
                string inputPath = RequestFolderStart();
                if (FolderExists(inputPath))
                {
                    List<FileInfo> allDirs = ProcessDirectory(inputPath);
                    ProcessFiles(allDirs);
                }
                else
                {
                    HandleMissingFolder(inputPath);
                }
                tryAgain = RequestRestart();
            } while (tryAgain);
            Console.ReadKey();
        }
        /// <summary>
        /// Ask user for a directory to search for duplicates.
        /// </summary>
        /// <returns>Path as string.</returns>
        private static string RequestFolderStart()
        {
            Console.WriteLine("Give path for folder to check (subfolders will also be checked): ");
            string inputPath = Console.ReadLine().Trim();
            int inputLength = inputPath.Length;
            // Removes quotation marks which "Copy as path" option adds
            if (inputPath[0] == '"' && inputPath[inputLength - 1] == '"')
            {
                inputPath = inputPath.Substring(1, inputLength - 2);
            }
            return inputPath;
        }

        /// <summary>
        /// Checks if the path provided is a valid directory.
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns>Result of if folder exists.</returns>
        private static bool FolderExists(string inputPath)
        {
            return Directory.Exists(inputPath);
        }
        /// <summary>
        /// Produce sensible error message for no directory found.
        /// </summary>
        /// <param name="inputPath"></param>
        private static void HandleMissingFolder(string inputPath)
        {
            Console.Clear();
            Console.WriteLine("{0} is not a valid directory.", inputPath);
        }
        /// <summary>
        /// Checks with user if to try again or exit.
        /// </summary>
        /// <returns>Result of user request.</returns>
        private static bool RequestRestart()
        {
            Console.WriteLine("Escape to shut down, any other key to continue.");
            ConsoleKeyInfo key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return false;
                default:
                    return true;
            }
        }
        /// <summary>
        /// Builds a list of files and directories and iterates through them.
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns>List of files found in directory and sub-directories</returns>
        private static List<FileInfo> ProcessDirectory(string inputPath)
        {
            List<FileInfo> fileList = new List<FileInfo>();
            Stack<string> dirs = new Stack<string>();
            dirs.Push(inputPath);
            Console.WriteLine("Building list of files...");
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs = null;
                string[] files = null;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                    files = Directory.GetFiles(currentDir);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Uh oh, something went wrong...");
                    Console.WriteLine(e.StackTrace);
                    continue;
                }
                // Assignment occurs in try block, requires check as not guarenteed to have happened
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        fileList.Add(new FileInfo(file));
                    }
                }
                if (subDirs != null)
                {
                    foreach (string str in subDirs)
                    {
                        dirs.Push(str);
                    }
                }
            }
            return fileList;
        }
        /// <summary>
        /// Compares file name and extension to find potential match.
        /// Passes result to FileCompare.
        /// Outputs result.
        /// </summary>
        /// <param name="fileList"></param>
        private static void ProcessFiles(List<FileInfo> fileList)
        {
            Console.Clear();
            int counter = 0;
            // Using StringBuilder more efficient than multiple string concatinations
            StringBuilder duplicates = new StringBuilder();
            for (int i = 0; i < fileList.Count; i++)
            {
                for (int j = i; j < fileList.Count; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }
                    else
                    {
                        if (fileList[i].Name == fileList[j].Name)
                        {
                            if (fileList[i].Extension == fileList[j].Extension)
                            {
                                /* Note: If the hash / checksum of file1's stream is saved before entering FileCompare
                                 *  then only the second file has to be streamed each iteration.
                                 */
                                if (FileCompare(fileList[i].FullName, fileList[j].FullName))
                                {
                                    duplicates.AppendLine(fileList[i].FullName + "\n" + fileList[j].FullName);
                                }
                            }
                        }
                    }
                }
                counter++;
                if (counter % 10 == 0)
                {
                    Console.WriteLine("Comparing file {0} / {1}", i + 1, fileList.Count);
                }
                if (counter == 2500)
                {
                    Console.Clear();
                    counter = 0;
                }
            }
            if (duplicates.Length == 0)
            {
                duplicates.Clear();
                duplicates.AppendLine("No duplicates located.");
            }
            else
            {
                Console.WriteLine("Duplicates: ");
                Console.WriteLine(duplicates);
            }
        }
        /// <summary>
        /// Compares file byte by byte to check for duplicates.
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <returns>Result if files are same.</returns>
        private static bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;
            if (file1 == file2)
            {
                return true;
            }
            try
            {
                fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
                fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);
                if (fs1.Length != fs2.Length)
                {
                    fs1.Close();
                    fs2.Close();
                    return false;
                }
                do
                {
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                }
                while ((file1byte == file2byte) && (file1byte != -1));

                fs1.Close();
                fs2.Close();
                return (file1byte == file2byte);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
