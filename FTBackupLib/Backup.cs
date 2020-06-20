using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTBackupLib
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

        public static string MakeBackupFilename(BackupParameters param)
        {
            string filename = param.backupName + (param.type == EBackupType.Full ? ".Full" : ".Inc") + param.backupTime.ToString(".yyyy-MM-ddTHHmmss") + ".zip";
            return Path.Combine(param.outputDirectory, filename);
        }

        public static void DoBackup(List<string> files, BackupParameters param)
        {
            string path = MakeBackupFilename(param);

            using (FileStream zipStream = new FileStream(path, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    foreach (var file in files)
                    {
                        archive.CreateEntryFromFile(file, file);
                    }
                }
            }
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