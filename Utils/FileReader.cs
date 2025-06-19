using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace _2c2pFileTransferAndStore.Utils
{
    public class FileReader
    {
        private readonly string _directoryPath;

        public FileReader(string directoryPath)
        {
            _directoryPath = directoryPath;
        }
            
        public bool DirectoryExists()
        {
            DirectoryInfo directory = new DirectoryInfo(_directoryPath);
            return directory.Exists;
        }

        public FileInfo[] GetFiles()
        {
            DirectoryInfo directory = new DirectoryInfo(_directoryPath);
            return directory.Exists ? directory.GetFiles() : Array.Empty<FileInfo>();
        }
        public int GetFileCount()
        {
            return GetFiles().Length;
        }
        public void DisplayFileSummary()
        {
            if (!DirectoryExists())
            {
                Console.WriteLine($"Directory '{_directoryPath}' does not exist.");
                return;

            }
            FileInfo[] files = GetFiles();
            if (files.Length == 0)
            {
                Console.WriteLine($"No files found in directory '{_directoryPath}'.\n"+
                                    "Please ensure the directory contains files."
                    );
                return;
            }
            
            Console.WriteLine("Files are");
            foreach (FileInfo file in files)
            {
                Console.WriteLine($"File Name : {file.Name}");
            }
            Console.WriteLine($"Total number of files: {files.Length}");
        }
        
    }
}
