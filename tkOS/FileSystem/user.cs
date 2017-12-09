using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem {
    public struct user {
        public ushort ID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string name;
        public ushort GID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] password;
        public bool block;

        public user(ushort ID, string name, ushort GID, byte[] password) {
            this.ID = ID;
            this.name = name;
            this.GID = GID;
            this.password = new byte[16];
            this.password = password;
            this.block = false;
        }

        public user(int ID, string name, int GID, byte[] password) {
            this.ID = (ushort)ID;
            this.name = name;
            this.GID = (ushort)GID;
            this.password = new byte[128];
            this.password = password;
            this.block = false;
        }

        public user(byte[] arr) {
            user str = new user();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (user)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            this = str;
        }

        public byte[] GetBytes() {
            int size = Marshal.SizeOf(this);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(this, ptr, false);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
    }
}
