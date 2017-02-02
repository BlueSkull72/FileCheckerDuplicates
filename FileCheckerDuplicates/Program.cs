using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
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
                    if (allDirs != null)
                    {
                        ProcessFiles(allDirs);
                    }
                    else
                    {
                        Console.WriteLine("Shutting down at user request.");
                    }
                }
                else
                {
                    HandleMissingFolder(inputPath);
                    tryAgain = RequestContinue();
                }
            } while (tryAgain);

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
        private static bool RequestContinue()
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
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
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
                catch (Exception)
                {
                    //nothing to see here, just failed to get access to a directory or file!
                    continue;
                }
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
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime listBuilding: " + elapsedTime);
            stopWatch.Reset();
            Console.WriteLine("Estimated time to complete is listBuilding times 30.");
            if (RequestContinue())
            {
                return fileList;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Compares file name and length to find potential match.
        /// Passes result to FileCompare.
        /// Outputs result.
        /// </summary>
        /// <param name="fileList"></param>
        private static void ProcessFiles(List<FileInfo> fileList)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.Clear();
            StringBuilder duplicates = new StringBuilder();
            while (fileList.Count > 0)
            {
                if (fileList[0] == null)
                {
                    fileList.TrimExcess();
                }
                else
                {
                    FileInfo toCheck = fileList[0];
                    fileList.RemoveAt(0);
                    fileList.TrimExcess();
                    bool addedOriginal = false;
                    for (int j = 0; j < fileList.Count; j++)
                    {
                        if (fileList[j] == null)
                        {
                            continue;
                        }
                        FileInfo checker = fileList[j];
                        try
                        {
                            if (toCheck.Name == checker.Name && toCheck.Extension == checker.Extension && toCheck.Length == checker.Length)
                            {
                                if (FileCompare(toCheck, checker))
                                {
                                    if (addedOriginal == false)
                                    {
                                        duplicates.AppendLine();
                                        duplicates.AppendLine(toCheck.FullName);
                                        duplicates.AppendLine(checker.FullName);
                                        addedOriginal = true;
                                    }
                                    else
                                    {
                                        duplicates.AppendLine(checker.FullName);
                                    }
                                    fileList.RemoveAt(j);
                                }
                            }
                        }
                        catch (Exception) { continue; }
                    }
                }
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("Files left to check against possible duplicates : {0}", fileList.Count);
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine("RunTime checkFiles: " + elapsedTime);
            stopWatch.Reset();
            if (!Directory.Exists(@"C:\Users\Public\temp"))
            {
                Directory.CreateDirectory(@"C:\Users\Public\temp");
            }
            using (StreamWriter writer = new StreamWriter(@"C:\Users\Public\temp\duplicateList.txt", false))
            {
                writer.Write(duplicates);
            }
            Console.WriteLine(@"List of possible duplicates written to C:\Users\temp\duplicateList.txt");
            Console.ReadKey();

        }
        /// <summary>
        /// Compares file binaries for duplicates.
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <returns>Result if files are same.</returns>
        const int BYTES_TO_READ = sizeof(Int64);
        private static bool FileCompare(FileInfo fileOne, FileInfo fileTwo)
        {
            int iterations = (int)Math.Ceiling((double)fileOne.Length / BYTES_TO_READ);
            try
            {
                using (FileStream fs1 = fileOne.OpenRead())
                using (FileStream fs2 = fileTwo.OpenRead())
                {
                    byte[] fileOneByteArray = new byte[BYTES_TO_READ];
                    byte[] fileTwoByteArray = new byte[BYTES_TO_READ];

                    for (int i = 0; i < iterations; i++)
                    {
                        fs1.Read(fileOneByteArray, 0, BYTES_TO_READ);
                        fs2.Read(fileTwoByteArray, 0, BYTES_TO_READ);

                        if (BitConverter.ToInt64(fileOneByteArray, 0) != BitConverter.ToInt64(fileTwoByteArray, 0))
                            return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                //unable to open either file for whatever reason, mark as "not dupes" and continue.
                return false;
            }
        }
    }
}