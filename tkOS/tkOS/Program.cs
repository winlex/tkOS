using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileSystem;

namespace tkOS {
    class Program {
        static void Main(string[] args) {
            FileSystem.FileSystem disk = null;
            while (true) {
                Console.Write(">");
                string key = Console.ReadLine();
                //try {
                    switch (key.Split(' ')[0]) {
                        case "CreateDisk": disk = new FileSystem.FileSystem(1024); break;
                        case "OpenDisk": disk = new FileSystem.FileSystem(key.Split(' ')[1]); break;
                        case "CloseDisk": disk.CloseDisk(); break;
                        case "ListFile": Console.WriteLine(disk.GetListFiles()); break;
                        case "FullListFile": Console.WriteLine(disk.GetListFilesFull()); break;
                        case "CreateFile": disk.CreateFile(key.Split(' ')[1], false); break;
                        case "DeleteFile": disk.DeleteFile(key.Split(' ')[1]); break;
                        case "RenameFile": disk.Rename(key.Split(' ')[1], key.Split(' ')[2]); break;
                        case "WriteFile": disk.WriteData(key.Split(' ')[1], Encoding.UTF8.GetBytes(key.Split(' ')[2])); break;
                        case "ReadFile": Console.WriteLine(Encoding.UTF8.GetString(disk.ReadData(key.Split(' ')[1], false))); break;
                        case "CurrentUser": Console.WriteLine(disk.CurrentUser.name); break;
                    }
                //} catch(Exception e) {
                //    if (disk == null) Console.WriteLine("Подключите диск!");
                //}
            }
        }
    }
}
