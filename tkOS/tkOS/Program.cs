using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSystem;

namespace tkOS {
    class Program {
        static void Main(string[] args) {
            FileSystem.FileSystem disk = new FileSystem.FileSystem(1024);
            while (true) {
                Console.Write(">");
                string key = Console.ReadLine();
                switch (key.Split(' ')[0]) {
                    case "ListFile": Console.WriteLine(disk.GetListFiles()); break;
                    case "CreateFile": disk.CreateFile(key.Split(' ')[1]); break;
                    case "DeleteFile": disk.DeleteFile(key.Split(' ')[1]); break;
                    case "RenameFile": disk.Rename(key.Split(' ')[1], key.Split(' ')[2]); break;
                    case "WriteFile": disk.WriteData(key.Split(' ')[1], key.Split(' ')[2]); break;
                    case "ReadFile": Console.WriteLine(disk.ReadData(key.Split(' ')[1], false)); break;
                }
            }
        }
    }
}
