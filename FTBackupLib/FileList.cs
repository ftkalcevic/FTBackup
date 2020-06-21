using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTBackupLib
{
    public class FileList
    {
        static int CAPACITY = 10000;
        SortedList<String, String> list = new SortedList<string, string>(CAPACITY);
        public void Add(string file)
        {
            if (list.Count == list.Capacity)
                list.Capacity = 2 * list.Capacity;
            if (!list.ContainsKey(file))
                list.Add(file, null);
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
