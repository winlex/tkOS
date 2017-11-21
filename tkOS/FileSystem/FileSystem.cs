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
        public string CurrentPosition = "/";
        public user CurrentUser;

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
                fs.Seek(SuperBlock.busy_kl * SuperBlock.size_kl, SeekOrigin.Begin);
                for (int i = 0; i < bmi.Length; i++)
                    fs.WriteByte(bmi[i]);
                for (int i = 0; i < bmi.Length / SuperBlock.size_kl; i++)
                    SuperBlock.busy_kl++;

                SuperBlock.move_ht = SuperBlock.busy_kl;
                Record[] hashTable = new Record[SuperBlock.count_kl];
                fs.Seek(SuperBlock.busy_kl * SuperBlock.size_kl, SeekOrigin.Begin);
                for (int i = 0; i < hashTable.Length; i++)
                    for (int j = 0; j < hashTable[i].GetBytes().Length; j++)
                        fs.WriteByte(hashTable[i].GetBytes()[j]);
                for (int i = 0; i < hashTable.Length * SuperBlock.size_in / SuperBlock.size_kl; i++)
                    SuperBlock.busy_kl++;

                //Тут будет запись списка кластеров
                

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
                Record[] hashTable = new Record[SuperBlock.count_kl];
                for(int i = 0; i < SuperBlock.count_kl; i++) {
                    byte[] block = new byte[SuperBlock.size_rec];
                    fs.Read(block, SuperBlock.move_ht+i* SuperBlock.size_rec, SuperBlock.size_rec);
                }
                int hashKey = name.GetHashCode() % 1024;
                while (hashTable[hashKey].name != "") {
                    if (hashKey == 1023)
                        hashKey = 0;
                    else
                        hashKey++;
                }
                Record record = new Record((ushort)hashKey, CurrentPosition + name, (ushort)indexI);
                inode inode = new inode(CurrentUser.ID, CurrentUser.GID, 0);
                bmi[indexI] = 1;
                fs.Seek(SuperBlock.move_bmi, SeekOrigin.Begin);
                fs.Write(bmi, 0, bmi.Length);
                fs.Seek(SuperBlock.move_in + indexI * SuperBlock.size_in, SeekOrigin.Begin);
                fs.Write(inode.GetBytes(), 0, SuperBlock.size_in);
                fs.Seek(SuperBlock.move_ht + hashKey * SuperBlock.size_rec, SeekOrigin.Begin);
                fs.Write(record.GetBytes(), 0, SuperBlock.size_rec);
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