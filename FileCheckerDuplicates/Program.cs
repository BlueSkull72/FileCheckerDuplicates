using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
                    tryAgain = RequestRestart();
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
                catch (Exception)
                {
                    //need to somehow eat this exception
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
            return fileList;
        }
        /// <summary>
        /// Compares file name and length to find potential match.
        /// Passes result to FileCompare.
        /// Outputs result.
        /// </summary>
        /// <param name="fileList"></param>
        private static void ProcessFiles(List<FileInfo> fileList)
        {
            Console.Clear();
            int counter = 0;
            StringBuilder duplicates = new StringBuilder();
            for (int i = 0; i < fileList.Count; i++)
            {
                if (fileList[i] == null)
                {
                    continue;
                }
                FileInfo toCheck = fileList[i];
                fileList.RemoveAt(i);
                bool addedOriginal = false;
                for (int j = 0; j < fileList.Count; j++)
                {
                    if (fileList[j] == null)
                    {
                        continue;
                    }
                    if (toCheck.Name == fileList[j].Name && toCheck.Extension == fileList[j].Extension && toCheck.Length == fileList[j].Length)
                    {
                        if (FileCompare(toCheck.FullName, fileList[j].FullName))
                        {
                            if (addedOriginal == false)
                            {
                                duplicates.AppendLine();
                                duplicates.AppendLine(toCheck.FullName);
                                duplicates.AppendLine(fileList[j].FullName);
                                addedOriginal = true;
                            }
                            else
                            {
                                duplicates.AppendLine(fileList[j].FullName);
                            }
                            fileList.RemoveAt(j);
                        }
                    }
                }
                counter++;
                if (counter % 10 == 0)
                {
                    Console.WriteLine("Checking file {0} against possible duplicates.", i);
                }
                if (counter == 2500)
                {
                    Console.Clear();
                    counter = 0;
                }
            }
            using (StreamWriter writer = new StreamWriter(@"c:\duplicateList.txt", false))
            {
                writer.Write(duplicates);
            }
            Console.WriteLine(@"List of duplicates written to c:\duplicateList.txt");
            Console.ReadKey();
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
