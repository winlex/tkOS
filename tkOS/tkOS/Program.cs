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
        }
    }
}
