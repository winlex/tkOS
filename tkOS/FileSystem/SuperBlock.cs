using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem {
    public struct SuperBlock {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        public string type; //Тип либо название

        public ushort size_kl; //Размер кластера в Кб
        public ushort count_kl; //Количество кластеров
        public ushort busy_kl; //Занятый кластер
        public ushort move_in;
        public ushort move_ht;
        public ushort move_bmi;
        public ushort size_in;
        public ushort size_rec;
        public ushort count_us;
        public ushort count_gr;
        public ushort size_us;
        public ushort size_gr;

        public SuperBlock(ushort size) {
            type = "tkOS";
            size_kl = 1024;
            count_kl = size;
            busy_kl = 0;
            move_in = 0;
            move_ht = 0;//?
            move_bmi = 0;//?
            size_in = 56;
            size_rec = 20;
            count_us = 2;
            count_gr = 2;
            size_us = 32;
            size_gr = 16;
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

        public SuperBlock(byte[] arr) {
            SuperBlock str = new SuperBlock();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (SuperBlock)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            this = str;
        }
    }
}
