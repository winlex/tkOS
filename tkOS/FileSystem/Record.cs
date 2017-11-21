using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem {
    public struct Record {
        public ushort ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string name;
        public ushort inode;

        public Record(int i) {
            this.ID = (ushort)i;
            name = "";
            inode = (ushort)i;
        }

        public Record(ushort ID, string name, ushort inode) {
            this.ID = ID;
            this.name = name;
            this.inode = inode;
        }

        public byte[] getBytes() {
            int size = Marshal.SizeOf(this);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, false);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public Record(byte[] arr) {
            Record str = new Record();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (Record)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            this = str;
        }
    }
}
