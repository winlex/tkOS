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
                string key = Console.ReadLine();
                switch (key.Split(' ')[0]) {
                    case "ls": Console.WriteLine(disk.GetListFiles()); break;
                    case "cat": disk.CreateFile(key.Split(' ')[1]); break;
                }
            }
        }
    }
}
