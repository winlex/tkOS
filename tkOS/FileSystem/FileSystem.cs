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
        public user[] users;
        public group[] groups;
        public Record[] MainCatalog;

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
                Record[] hashTable = new Record[64];
                fs.Seek(SuperBlock.busy_kl * SuperBlock.size_kl, SeekOrigin.Begin);
                for (int i = 0; i < hashTable.Length; i++)
                    for (int j = 0; j < hashTable[i].GetBytes().Length; j++)
                        fs.WriteByte(hashTable[i].GetBytes()[j]);
                for (int i = 0; i < hashTable.Length * SuperBlock.size_in / SuperBlock.size_kl; i++)
                    SuperBlock.busy_kl++;
                MainCatalog = hashTable;
                
                fs.Seek(SuperBlock.size_kl, SeekOrigin.Begin);
                for (int i = 0; i < SuperBlock.busy_kl; i++)
                    listKlaster[i] = -1;
                foreach (short t in listKlaster) 
                    fs.Write(BitConverter.GetBytes(t), 0, sizeof(short));
                

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
                //////////////////////////////////////
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
                fs.Close();
            }
            return 0;
        }
        public int DeleteFile(string name) {
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                Record[] hashTable = new Record[SuperBlock.count_kl];
                for (int i = 0; i < SuperBlock.count_kl; i++) {
                    byte[] block = new byte[SuperBlock.size_rec];
                    fs.Read(block, SuperBlock.move_ht + i * SuperBlock.size_rec, SuperBlock.size_rec);
                }
                //////////////////////////////////////
                int hashKey = name.GetHashCode() % 1024;
                while (hashTable[hashKey].name != name) {
                    if (hashKey == 1023)
                        hashKey = 0;
                    else
                        hashKey++;
                }
                hashTable[hashKey].name = "";
                byte[] arrinode = new byte[SuperBlock.size_in];
                fs.Seek(SuperBlock.move_in + SuperBlock.size_in * hashTable[hashKey].inode, SeekOrigin.Begin);
                fs.Close();
            }
            return 0;
        }
        public int CopyFile(ushort num) {
            return 0;
        }
        public short[] ReadFatTable() {
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(SuperBlock.size_kl, SeekOrigin.Begin);
                short[] temp = new short[SuperBlock.count_kl];
                for(int i = 0; i < SuperBlock.count_kl; i++) {
                    byte[] block = new byte[sizeof(short)];
                    fs.Read(block, 0, sizeof(short));
                    temp[i] = BitConverter.ToInt16(block,0);
                }
                fs.Close();
                return temp;
            }
        }
        public void WriteFatTable(short[] temp) {
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(SuperBlock.size_kl, SeekOrigin.Begin);
                foreach (short t in temp)
                    fs.Write(BitConverter.GetBytes(t), 0, sizeof(short));
                fs.Close();
            }

        }
        public Record[] ReadHashTable() {
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(SuperBlock.move_ht * SuperBlock.size_kl, SeekOrigin.Begin);
                Record[] temp = new Record[64];
                for (int i = 0; i < temp.Length; i++) {
                    byte[] block = new byte[SuperBlock.size_rec];
                    fs.Read(block, 0, SuperBlock.size_rec);
                    temp[i] = new Record(block);
                }
                fs.Close();
                return temp;
            }
        }
        public void WriteHashTable(Record[] temp) {
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(SuperBlock.move_ht * SuperBlock.size_kl, SeekOrigin.Begin);
                for (int i = 0; i < temp.Length; i++)
                    for (int j = 0; j < temp[i].GetBytes().Length; j++)
                        fs.WriteByte(temp[i].GetBytes()[j]);
                fs.Close();
            }

        }
        public inode ReadInode(short index) {
            byte[] temp = new byte[SuperBlock.size_in];
            using(FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(SuperBlock.move_in + index * SuperBlock.size_in,SeekOrigin.Begin);
                fs.Read(temp, 0, SuperBlock.size_in);
                fs.Close();
            }
            return new inode(temp);
        }
        public void WriteInode(inode inode, short index) {
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(SuperBlock.move_in + index * SuperBlock.size_in, SeekOrigin.Begin);
                fs.Write(inode.GetBytes(), 0, SuperBlock.size_in);
                fs.Close();
            }
        }
    }


}