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
        private static void AppendDirectory(ref List<String> list, FileFilter filter, BackupParameters param, DirectoryInfo di)
        {
            FileInfo[] files = di.GetFiles();
            foreach (var f in files)
            {
                AppendFile(ref list, filter, param, f);
            }
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (var d in dirs)
            {
                AppendDirectory(ref list, filter, param, d);
            }
        }

        private static void AppendFile(ref List<String> list, FileFilter filter, BackupParameters param, FileInfo f)
        {
            if ( ( param.type == EBackupType.Incremental && f.LastWriteTime > param.lastBackup ) || param.type == EBackupType.Full )
                if (!filter.Match(f.Name))
                {
                    list.Add(f.FullName);
                }
        }

        public static List<String> GetFiles(FileSelectionList list, FileFilter filter, BackupParameters param)
        {
            List<String> files = new List<string>();

            foreach (var path in list)
            {
                FileInfo f = new FileInfo(path);
                if (f.Exists)
                {
                    AppendFile(ref files, filter, param, f);
                }
                else
                {
                    DirectoryInfo d = new DirectoryInfo(path);
                    if (d.Exists)
                    {
                        AppendDirectory(ref files, filter, param, d);
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