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
            string output = "DUPLICATES : ";
            DirectoryInfo dir1 = new DirectoryInfo(inputPath);
            FileInfo[] list1 = dir1.GetFiles("*.*", SearchOption.AllDirectories);
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
                output += "\n";
            }
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
    }
}
