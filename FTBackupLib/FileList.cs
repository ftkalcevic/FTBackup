using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTBackupLib
{
    public class FileList
    {
        static int CAPACITY = 10000;
        SortedList<String, String> list = new SortedList<string, string>(CAPACITY);
        public UInt64 totalBytes=0;

        public void Add(FileInfo file)
        {
            totalBytes += (UInt64)file.Length;
            if (list.Count == list.Capacity)
                list.Capacity = 2 * list.Capacity;
            if (!list.ContainsKey(file.FullName))
                list.Add(file.FullName, null);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return list.Keys.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }
    }
}
