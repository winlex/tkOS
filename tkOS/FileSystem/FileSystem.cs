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
        public Record[] MainCatalog;

        public FileSystem(ushort size) {
            SuperBlock = new SuperBlock(size);

            //Забиваю нулями файл
            File.WriteAllBytes(SuperBlock.count_kl + ".disk", new byte[SuperBlock.count_kl * SuperBlock.size_kl]);
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk",FileMode.Open)) {
                //Запись суперблока
                fs.Seek(0, SeekOrigin.Begin);
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
                //Сдесь указывается размер каталога
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

            user[] user = new user[5];
            CurrentUser = new user(0, "root", 0, new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes("root")));
            user[0] = CurrentUser;
            user[1] = new user(1, "guest", 1, new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes("")));
            group[] group = new group[5];
            group[0] = new group(0, "admins");
            group[1] = new group(1, "guests");
            CreateFile("users", true);
            CreateFile("groups", true);
            WriteUsers(user);
            WriteGroups(group);

            CurrentUser = user[1];
        }
        public FileSystem(string path) {
            using (FileStream fs = File.Open(path, FileMode.Open)) {
                byte[] temp = new byte[24];
                fs.Seek(0, SeekOrigin.Begin);
                fs.Read(temp, 0, 24);
                SuperBlock = new SuperBlock(temp);
                fs.Close();
            }
        }
        public void CloseDisk() {
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(SuperBlock.GetBytes(), 0, 24);
                fs.Close();
            }
        }

        public int CreateFile(string name, bool block) {
            Record[] hashTable = ReadHashTable();
            int indexI;
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                byte[] bmi = new byte[SuperBlock.count_kl];
                fs.Seek(SuperBlock.move_bmi, SeekOrigin.Begin);
                fs.Read(bmi, 0, SuperBlock.count_kl);
                indexI = bmi.ToList<byte>().IndexOf(0);
                if (indexI == -1)
                    return 1;
                //////////////////////////////////////
                bmi[indexI] = 1;
                fs.Seek(SuperBlock.move_bmi, SeekOrigin.Begin);
                fs.Write(bmi, 0, SuperBlock.size_kl);
                fs.Close();
            }
            int hashKey = name.GetHashCode() % hashTable.Length;
            if (hashKey < 0) hashKey *= -1;
            while (hashTable[hashKey].name != "") {
                if (hashKey == hashTable.Length-1)
                    hashKey = 0;
                else
                    hashKey++;
            }
            Record record = new Record((ushort)hashKey, name, (ushort)indexI);
            hashTable[hashKey] = record;
            inode inode = new inode(CurrentUser.ID, CurrentUser.GID, 0);
            WriteInode(inode, indexI);
            WriteHashTable(hashTable);
            return 0;
        }
        public int DeleteFile(string name) {
            Record[] hashTable = ReadHashTable();
            int hashKey = name.GetHashCode() % hashTable.Length;
            if (hashKey < 0) hashKey *= -1;
            while (hashTable[hashKey].name != name) {
                if (hashKey == hashTable.Length-1)
                    hashKey = 0;
                else
                    hashKey++;
            }
            hashTable[hashKey].name = "";
            inode inode = ReadInode(hashTable[hashKey].inode);
            short[] fatTable = ReadFatTable();
            short t = inode.adr;
            inode.adr = -1;
            SuperBlock.busy_kl--;
            while(t != -1) {
                t = fatTable[t];
                fatTable[t] = -1;
                SuperBlock.busy_kl--;
            }
            WriteFatTable(fatTable);
            WriteInode(inode, hashTable[hashKey].inode);
            WriteHashTable(hashTable);
            return 0;
        }
        public int Rename(string name, string rename) {
            Record[] hashTable = ReadHashTable();
            int hashKey = name.GetHashCode() % hashTable.Length;
            if (hashKey < 0) hashKey *= -1;
            while (hashTable[hashKey].name != name) {
                if (hashKey == hashTable.Length-1)
                    hashKey = 0;
                else
                    hashKey++;
            }
            hashTable[hashKey].name = rename;
            WriteHashTable(hashTable);

            return 0;
        }
        public int WriteData(string name, byte[] data) {
            double needKl = (double)data.Length / (double)SuperBlock.size_kl;
            int t = Convert.ToInt16(Math.Ceiling(needKl));
            if (t + SuperBlock.busy_kl > SuperBlock.count_kl)
                return 1;
            Record[] hashTable = ReadHashTable();
            int hashKey = name.GetHashCode() % hashTable.Length;
            if (hashKey < 0) hashKey *= -1;
            while (hashTable[hashKey].name != name) {
                if (hashKey == hashTable.Length-1)
                    hashKey = 0;
                else
                    hashKey++;
            }
            inode inode = ReadInode(hashTable[hashKey].inode);
            short[] fatTable = ReadFatTable();
            int index = fatTable.ToList<short>().IndexOf(0);
            inode.adr = (short)index;
            fatTable[index] = -1;
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(index * SuperBlock.size_kl, SeekOrigin.Begin);
                try {
                    fs.Write(data, 0, SuperBlock.size_kl);
                } catch (Exception e) {
                    fs.Write(data, 0, data.Length);
                }
                for (int i = 1; i < t; i++) {
                    fatTable[index] = (short)fatTable.ToList<short>().IndexOf(0);
                    index = fatTable.ToList<short>().IndexOf(0);
                    fs.Seek(index * SuperBlock.size_kl, SeekOrigin.Begin);
                    try {
                        fs.Write(data, i * SuperBlock.size_in, SuperBlock.size_in);
                    } catch (Exception e) {
                        fs.Write(data, i * SuperBlock.size_in, data.Length % SuperBlock.size_kl);
                    }
                    fatTable[index] = 0;
                }
                fs.Close();
            }

            inode.size = (uint)data.Length;
            WriteInode(inode, hashTable[hashKey].inode);
            WriteFatTable(fatTable);
            return 0;
        }
        public byte[] ReadData(string name, bool fl) {
            byte[] data;

            Record[] hashTable = ReadHashTable();
            int hashKey = name.GetHashCode() % hashTable.Length;
            if (hashKey < 0) hashKey *= -1;
            while (hashTable[hashKey].name != name) {
                if (hashKey == hashTable.Length - 1)
                    hashKey = 0;
                else
                    hashKey++;
            }
            inode inode = ReadInode(hashTable[hashKey].inode);
            data = new byte[inode.size];
            short[] fatTable = ReadFatTable();

            if (inode.adr != -1) {
                using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                    fs.Seek(inode.adr * SuperBlock.size_kl, SeekOrigin.Begin);
                    try {
                        fs.Read(data, 0, SuperBlock.size_kl);
                    } catch (Exception e) {
                        fs.Read(data, 0, (int)inode.size);
                    }
                    int t = fatTable[inode.adr];
                    if (fl) inode.adr = -1;
                    int i = 1;
                    while (t != -1) {
                        fs.Seek(t * SuperBlock.size_kl, SeekOrigin.Begin);
                        try {
                            fs.Read(data, i *SuperBlock.size_kl, SuperBlock.size_kl);
                        } catch (Exception e) {
                            fs.Read(data, i * SuperBlock.size_kl, (int)inode.size % SuperBlock.size_kl);
                        }
                        int m = t;
                        t = fatTable[t];
                        if (fl) fatTable[m] = -1;
                        i++;
                    }
                    fs.Close();
                }
            }

            return data; ;
        }
        public string GetListFiles() {
            string result = "";
            foreach (Record t in ReadHashTable())
                if (t.name != "")
                    result += t.name + "\t";
            return result;
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
                //Сдесь указывается размер каталога
                Record[] temp = new Record[64];
                for (int i = 0; i < temp.Length; i++) {
                    byte[] block = new byte[SuperBlock.size_rec];
                    fs.Seek(SuperBlock.move_ht * SuperBlock.size_kl + i * SuperBlock.size_rec, SeekOrigin.Begin);
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
        public inode ReadInode(int index) {
            byte[] temp = new byte[SuperBlock.size_in];
            using(FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(SuperBlock.move_in + index * SuperBlock.size_in,SeekOrigin.Begin);
                fs.Read(temp, 0, SuperBlock.size_in);
                fs.Close();
            }
            return new inode(temp);
        }
        public void WriteInode(inode inode, int index) {
            using (FileStream fs = File.Open(SuperBlock.count_kl + ".disk", FileMode.Open)) {
                fs.Seek(SuperBlock.move_in + index * SuperBlock.size_in, SeekOrigin.Begin);
                fs.Write(inode.GetBytes(), 0, SuperBlock.size_in);
                fs.Close();
            }
        }
        public void WriteUsers(user[] user) {
            byte[] temp = new byte[SuperBlock.count_us*SuperBlock.size_us];
            for (int i = 0; i < SuperBlock.count_us; i++)
                Array.Copy(user[i].GetBytes(), 0, temp, i * SuperBlock.size_us, SuperBlock.size_us);
            WriteData("users", temp);
        }
        public void WriteGroups(group[] group) {
            byte[] temp = new byte[SuperBlock.count_gr * SuperBlock.size_gr];
            for (int i = 0; i < SuperBlock.count_gr; i++)
                Array.Copy(group[i].GetBytes(), 0, temp, i * SuperBlock.size_gr, SuperBlock.size_gr);
            WriteData("groups", temp);
        }
    }


}