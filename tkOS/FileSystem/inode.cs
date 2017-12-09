using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileSystem {
    public struct inode {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
        public string permissions;
        public bool block;
        public ushort type; 
        public DateTime dateCreate;
        public DateTime dateModify;
        public ushort UID;
        public ushort GID;
        public uint size;
        public short adr;

        public inode(ushort UID, ushort GID, ushort type) {
            this.type = type;
            this.permissions = "rwx------";
            this.block = false;
            this.dateCreate = DateTime.Now;
            this.dateModify = DateTime.Now;
            this.UID = UID;
            this.GID = GID;
            this.size = 0;
            this.adr = -1; ;
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
