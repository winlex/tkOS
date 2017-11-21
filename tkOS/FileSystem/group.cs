using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem {
    public struct group {
        public ushort ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string name;
        public bool block;

        public group(ushort ID, string name) {
            this.ID = ID;
            this.name = name;
            block = false;
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

        public group(byte[] arr) {
            group str = new group();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (group)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            this = str;
        }

        public group(int v, string text) : this() {
            this.ID = (ushort)v;
            this.name = text;
        }
    }
}
