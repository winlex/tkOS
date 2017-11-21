using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileSystem {
    public struct inode {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public bool[] permissions;
        public bool block;
        public char type; //0 - свободный, 1 - файл
        public DateTime dateCreate;
        public DateTime dateModify;
        public ushort UID;
        public ushort GID;
        public uint size;
        public ushort adr;

        public inode(ushort UID, ushort GID) {
            this.type = '0';
            this.permissions = new bool[9];
            this.permissions[0] = this.permissions[1] = this.permissions[2] = true;
            this.block = false;
            this.dateCreate = DateTime.Now;
            this.dateModify = DateTime.Now;
            this.UID = UID;
            this.GID = GID;
            this.size = 0;
            this.adr = 0; ;
        }

        public inode(byte[] arr) {
            inode str = new inode();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (inode)Marshal.PtrToStructure(ptr, str.GetType());
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
