using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Security.Cryptography;

namespace FileSystem {
    public class FileSystem {
        public SuperBlock SuperBlock;
        public Record mainCatalog;

        public FileSystem(ushort size) {
            SuperBlock = new SuperBlock(size);

            //Забиваю нулями файл
            File.WriteAllBytes(SuperBlock.count_kl + ".disk", new byte[SuperBlock.count_kl * SuperBlock.size_kl]);
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk",FileMode.Open)) {
                //Запись суперблока
                fs.Write(SuperBlock.GetBytes(), 0, SuperBlock.GetBytes().Length);//Отмечаем что один кластер уже занят 
                SuperBlock.busy_kl++;

                //Создание списка свободного/занятых кластеров
                short[] listKlaster = new short[SuperBlock.count_kl];
                for (int i = 0; i < sizeof(short) * listKlaster.Length / SuperBlock.size_kl; i++) //В зависимости от объема памяти, вычесляем колиество занимаемых кластеров
                    SuperBlock.busy_kl++;

                SuperBlock.move_in = SuperBlock.busy_kl;
                //Создаем иЛист, сразу отмечаем кластеры, которые заняты инодами, в дальнейшем мы отметим это и в битовой карте
                inode[] ilist = new inode[SuperBlock.count_kl];
                fs.Seek(SuperBlock.busy_kl * SuperBlock.size_kl, SeekOrigin.Begin);
                for (int i = 0; i < ilist.Length; i++)
                    for (int j = 0; j < ilist[i].GetBytes().Length; j++)
                        fs.WriteByte(ilist[i].GetBytes()[j]);
                for (int i = 0; i < ilist.Length * SuperBlock.size_in / SuperBlock.size_kl; i++)
                    SuperBlock.busy_kl++;

                SuperBlock.move_bmi = SuperBlock.busy_kl;
                //Создаем битовую карту инодов
                byte[] bmi = new byte[SuperBlock.count_kl];
                bmi[0] = 1;
                fs.Seek(SuperBlock.busy_kl * SuperBlock.size_kl, SeekOrigin.Begin);
                for (int i = 0; i < bmi.Length; i++)
                    fs.WriteByte(bmi[i]);
                for (int i = 0; i < bmi.Length / SuperBlock.size_kl; i++)
                    SuperBlock.busy_kl++;

                //Тут будет запись списка кластеров

                SuperBlock.move_mc = SuperBlock.busy_kl;

                fs.Close();
            }

        }

        public FileSystem(string path) {
        }
        public int CreateFile(string name) {
            using(FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                byte[] bmi = new byte[SuperBlock.count_kl];
                fs.Read(bmi, SuperBlock.move_bmi, SuperBlock.count_kl);
                int indexI = bmi.ToList<byte>().IndexOf(0);
                if (indexI == -1)
                    return 1;

            }
            return 0;
        }
        public int DeleteFile(ushort num) {
            return 0;
        }
        public int CopyFile(ushort num) {
            return 0;
        }
    }


}