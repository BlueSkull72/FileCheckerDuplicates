using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileCheckerDuplicates
{
    class Program
    {
        static void Main(string[] args)
        {
            RequestFolderStart();
            Console.ReadKey();
        }
        private static void RequestFolderStart()
        {
            Console.WriteLine("Give path for folder to check (subfolders will also be checked): ");
            string inputPath = Console.ReadLine().Trim();
            if (Directory.Exists(inputPath))
            {
                ProcessDirectory(inputPath);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("{0} is not a valid directory.", inputPath);
                Console.WriteLine("Escape to shut down, any other key to continue.");
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        break;
                    default:
                        RequestFolderStart();
                        break;
                }
            }
        }

        private static void ProcessDirectory(string inputPath)
        {
            List<FileInfo> fileList = new List<FileInfo>();
            Stack<string> dirs = new Stack<string>();
            dirs.Push(inputPath);
            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;
                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch
                {
                    continue;
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch
                {
                    continue;
                }
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        fileList.Add(new FileInfo(file));
                    }
                }
                foreach (string str in subDirs)
                {
                    dirs.Push(str);
                }
            }
            ProcessFiles(fileList);
        }
        private static void ProcessFiles(List<FileInfo> fileList)
        {

            string output = "DUPLICATES : ";
            FileInfo[] list1 = fileList.ToArray();
            for (int i = 0; i < list1.Length; i++)
            {
                for (int j = i; j < list1.Length; j++)
                {
                    if (j == i)
                    {
                        continue;
                    }
                    else
                    {
                        if (FileCompare(list1[i].FullName, list1[j].FullName))
                        {
                            output += "\n" + list1[i].FullName + "\n" + list1[j].FullName;
                        }
                    }
                }
                if (i % 100 == 0)
                {
                    Console.Clear();
                    Console.WriteLine(((float)i / (float)list1.Length) * 100 + "% completed.");
                }
                output += "\n";
            }
            if (output == "DUPLICATES : ")
            {
                output = "No duplicates located.";
            }
            Console.Clear();
            Console.WriteLine(output);
        }
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
                return ((file1byte - file2byte) == 0);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
