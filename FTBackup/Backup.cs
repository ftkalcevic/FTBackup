using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTBackup
{
    public class Backup
    {
        private void AppendDirectory(ref List<String> list, FileFilter filter, BackupParameters param, FileInfo f)
        {
        }

        public static List<String> GetFiles(FileSelectionList list, FileFilter filter, BackupParameters param)
        {
            List<String> files = new List<string>();

            foreach (var path in list)
            {
                FileInfo f = new FileInfo(path);
                if (f.Exists)
                {
                    if (f.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        AppendDirectory(ref files, filter, param, f);
                    }
                    else
                    {
                    }
                }
            }
            return files;
        }
    }
}


/*
- UI to...
    - select folders, files, etc
    - select filters
    - backup
    - restore
- Inputs
    - output location
    - backup type - inc/full
    - last backup timestamp
    - input files/paths
    - input file filter


 */